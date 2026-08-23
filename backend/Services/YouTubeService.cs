using Microsoft.Extensions.Options;
using MyApi.Config;
using MyApi.DTOs;
using MyApi.Interfaces;
using MyApi.Models;
using MyApi.Raws.Channel;
using MyApi.Raws.Search;
using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Net.WebRequestMethods;

namespace MyApi.Services;

public class YouTubeService : IYouTubeService
{
    private readonly IRedisCacheService _cache;
    private readonly HttpClient _httpClient;
    private readonly YouTubeApiSettings _settings;
    private readonly ILogger<YouTubeService> _logger;

    public YouTubeService(
        IRedisCacheService cache,
        HttpClient httpClient,
        IOptions<YouTubeApiSettings> settings,
        ILogger<YouTubeService> logger)
    {
        _cache = cache;
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// API を使って動画データを取得
    /// </summary>
    /// <returns></returns>
    public async Task<VideoDataResponse> FetchYouTubeLiveStreamsAsync()
    {
        // まずは配信を取得
        var searchResponse = await GetYouTubeLiveStreamsAsync();
        if (searchResponse.Items.Count == 0)
        {
            return new();
        }

        // 次にチャンネル情報を取得
        var channelResponse = await GetChannelInformationAsync(searchResponse.Items);

        // dto に整形
        var dto = ToDTO(searchResponse.Items, channelResponse.Items);
        return dto;
    }

    private async Task<YouTubeSearchResponse> GetYouTubeLiveStreamsAsync()
    {
        var allResponse = new YouTubeSearchResponse();

        var baseUrl = GetSerachListUrl();

        //今後 api 制限が緩和したら増やす
        var searchCount = 2;
        var nextPageToken = "";

        for (var i = 0; i < searchCount; i++)
        {
            var url = baseUrl;

            // pageToken を更新
            if (!string.IsNullOrEmpty(nextPageToken))
            {
                url += $"&pageToken={nextPageToken}";
            }

            // response を取得
            var (httpResponse, msg) = await GetHttpResponseWithRetryAsync(url, "search list");
            if (httpResponse is null)
            {
                ShowLog(msg);
                break;
            }

            using (httpResponse)
            {
                // json でデータを取得
                var json = await httpResponse.Content.ReadAsStringAsync();
                // デシリアライズ
                var searchResponse = JsonSerializer.Deserialize<YouTubeSearchResponse>(json);
                if (searchResponse is null)
                {
                    break;
                }

                // 企業勢のフィルタリストを取得
                var filter = await _cache.GetVtuberFilterListAsync(
                    CacheKeyHelper.GetVtuberCachKey(VIdeoPlatform.YouTube));
                // フィルタリング 企業勢をはじく
                var filterResponse = searchResponse.Items
                    .Where(v => !filter.Contains(v.Snippet.ChannelId));

                // 結果に追加
                allResponse.Items.AddRange(filterResponse);
                if (string.IsNullOrEmpty(searchResponse.NextPageToken))
                {
                    break;
                }
                nextPageToken = searchResponse.NextPageToken;
            }
        }

        // 重複を除き、日本語配信に限定
        // 動画タイトル、チャンネルタイトルどちらかに日本語が入っていれば OK とする
        allResponse.Items = allResponse.Items
            .DistinctBy(v => v.Snippet.ChannelId)
            .Where(v => HasJapanese(v.Snippet.Title)
                     || HasJapanese(v.Snippet.ChannelTitle))
            .ToList();
        return allResponse;
    }

    private string GetSerachListUrl()
    {
        return "https://www.googleapis.com/youtube/v3/search" +
            $"?part=snippet" +
            $"&type=video" +
            $"&eventType=live" +
            $"&regionCode=JP" +
            $"&relevanceLanguage=ja" +
            $"&maxResults=50" +
            $"&q=Vtuber" +
            $"&key={_settings.ApiKey}";
    }

    private static bool HasJapanese(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        // ひらがな・カタカナ・漢字
        return Regex.IsMatch(text, @"[\u3040-\u30FF\u4E00-\u9FFF]");
    }

    private async Task<YouTubeChannelsResponse> GetChannelInformationAsync(List<YouTubeSearchItemRaw> items)
    {
        // チャンク分け
        var channelIdBatches = items
            .Select(x => x.Snippet.ChannelId)
            .Chunk(50)
            .ToList();

        var allResults = new YouTubeChannelsResponse();

        foreach (var batch in channelIdBatches)
        {
            var ids = string.Join(",", batch);

            var url = GetChannelsListUrl(ids);

            var (httpResponse, msg) = await GetHttpResponseWithRetryAsync(url, "channels list");
            if (httpResponse is null)
            {
                ShowLog(msg);
                continue;
            }

            using (httpResponse)
            {
                var json = await httpResponse.Content.ReadAsStringAsync();
                var channelResponse = JsonSerializer.Deserialize<YouTubeChannelsResponse>(json);

                if (channelResponse is not null)
                {
                    allResults.Items.AddRange(channelResponse.Items);
                }
            }
        }

        return allResults;
    }

    private string GetChannelsListUrl(string channelIds)
    {
        return $"https://www.googleapis.com/youtube/v3/channels" +
                $"?part=snippet,brandingSettings,topicDetails" +
                $"&id={channelIds}" +
                $"&key={_settings.ApiKey}";
    }

    /// <summary>
    /// httpResponse を取得。 503 の場合は最大 3 回リトライできる
    /// </summary>
    /// <param name="url"></param>
    /// <param name="requestMethodName"></param>
    /// <returns></returns>
    private async Task<(HttpResponseMessage? response, string errorMsg)> GetHttpResponseWithRetryAsync(
        string url,
        string requestMethodName)
    {
        var maxRetry = 3;

        // 初回 + 最大3回のリトライ＝ 最大4回試行
        for (var tryCount = 0; tryCount <= maxRetry; tryCount++)
        {
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return (response, "");
            }

            using (response)
            {
                // 503 かつ リトライ可能回数が残っている場合のみ待機
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    if (tryCount != maxRetry)
                    {
                        // 最大で 2 + 4 + 8 = 14 秒待機
                        var delaySeconds = (int)Math.Pow(2, tryCount + 1);
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                        continue;
                    }
                }
                // 503 以外のエラーは即 return null
                else if (!response.IsSuccessStatusCode)
                {
                    return (null, $"{requestMethodName} {response.StatusCode}エラー。");
                }
            }
        }

        return (null, $"{requestMethodName} 503エラー。");
    }

    /// <summary>
    /// dto へ変換
    /// </summary>
    /// <param name="sItems"></param>
    /// <param name="cItems"></param>
    /// <returns></returns>
    private VideoDataResponse ToDTO(List<YouTubeSearchItemRaw> sItems, List<YouTubeChannelItemRaw> cItems)
    {
        return new VideoDataResponse
        {
            Items = sItems
                .Join(
                    cItems,
                    s => s.Snippet.ChannelId,
                    c => c.Id,
                    (s, c) => new VideoDataDTO
                    {
                        VideoId = s.Id.VideoId,
                        VideoTitle = s.Snippet.Title,
                        ChannelId = s.Snippet.ChannelId,
                        ChannelName = s.Snippet.ChannelTitle,
                        SearchDescription = s.Snippet.Description,
                        ChannelDescription = c.Snippet.Description,
                        SearchHighTumbnail = new VideoHighThumbnailDTO
                        {
                            Url = $"https://i.ytimg.com/vi/{s.Id.VideoId}/hq720.jpg",//s.Snippet.Thumbnails.High.Url,
                            Width = 1280,//s.Snippet.Thumbnails.High.Width,
                            Height = 720,//s.Snippet.Thumbnails.High.Height,
                        },
                        ChannelHighThumbnail = new VideoHighThumbnailDTO
                        {
                            Url = c.Snippet.Thumbnails.High.Url,
                            Width = c.Snippet.Thumbnails.High.Width,
                            Height = c.Snippet.Thumbnails.High.Height
                        },
                        Keywords = c.BrandingSettings.Channel.Keywords,
                        BannerExternalUrl = c.BrandingSettings.Image.BannerExternalUrl,
                        TopicCategories = c.TopicDetails.TopicCategories
                            .Select
                            (
                                // AI と人間が読みやすいように アンダーバー、特殊文字を修正
                                url => System.Net.WebUtility.UrlDecode(
                                url.Split('/')
                                .Last()
                                .Replace("_", " "))
                            )
                            .ToList(),
                        Platform = VIdeoPlatform.YouTube
                    }
                )
                .ToList()
        };
    }

    private void ShowLog(string message)
    {
        var time = DateTime.Now;
        _logger.LogInformation("\n{Time}{Message}\n", time, message);
    }
}