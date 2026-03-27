using Microsoft.Extensions.Options;
using MyApi.Config;
using MyApi.DTOs;
using MyApi.Interfaces;
using MyApi.Models;
using System.Text.Json;
using System.Text.RegularExpressions;
using MyApi.Raws.Search;
using MyApi.Raws.Channel;

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

    public async Task<VideoDataResponse> SearchYouTubeLiveStreamsAsync()
    {
        var cacheKey = CacheKeyHelper.GetCacheKey(CacheKeyHelper.VideoType.YouTubeLiveStream);
        return await _cache.GetAsync<VideoDataResponse>(cacheKey) ?? new();
    }

    

    public async Task<VideoDataResponse> FetchYouTubeLiveStreamsAsync()
    {
        var searchResponse = await GetYouTubeLiveStreamsAsync();
        if(searchResponse.Items.Count == 0)
        {
            return new();
        }

        var channelResponse = await GetChannelInformationAsync(searchResponse.Items);
        var dto = ToDTO(searchResponse.Items, channelResponse.Items);
        dto.Items = dto.Items
            .Where(v => !IndividualChecker.IsCompany(v.ChannelDescription))
            .ToList();
        return dto;
    }

    private async Task<YouTubeSearchResponse> GetYouTubeLiveStreamsAsync()
    {
        var allResponse = new YouTubeSearchResponse();

        var baseUrl =
            "https://www.googleapis.com/youtube/v3/search" +
            $"?part=snippet" +
            $"&type=video" +
            $"&eventType=live" +
            $"&regionCode=JP" +
            $"&relevanceLanguage=ja" +
            $"&maxResults=50" +
            $"&q=Vtuber" +
            $"&key={_settings.ApiKey}";

        var nextPageToken = "";
        //今後 api 制限が緩和したら増やす
        var searchCount = 2; 
        for (var i = 0; i < searchCount; i++)
        {
            var url = baseUrl;

            if(!string.IsNullOrEmpty(nextPageToken))
            {
                url += $"&pageToken={nextPageToken}";
            }

            var httpResponse = await GetHttpResponseMessageWithRetryAsync(url, "search list");
            if(httpResponse is null)
            {
                continue;
            }

            var json = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<YouTubeSearchResponse>(json);
            if (response is null)
            {
                break;
            }

            allResponse.Items.AddRange(response.Items);
            if(string.IsNullOrEmpty(response.NextPageToken))
            {
                break;
            }
            nextPageToken = response.NextPageToken;
        }

        // 重複を除き、日本語配信に限定
        allResponse.Items = allResponse.Items
            .DistinctBy(v => v.Snippet.ChannelId)
            .Where(v => HasJapanese(v.Snippet.Title)
                     || HasJapanese(v.Snippet.ChannelTitle))
            .ToList();
        return allResponse;
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
        var channelIdBatches = items
            .Select(x => x.Snippet.ChannelId)
            .Chunk(50)
            .ToList();

        var allResults = new YouTubeChannelsResponse();

        foreach (var batch in channelIdBatches)
        {
            var ids = string.Join(",", batch);

            var url =
                $"https://www.googleapis.com/youtube/v3/channels" +
                $"?part=snippet,brandingSettings,topicDetails" +
                $"&id={ids}" +
                $"&key={_settings.ApiKey}";

            var httpResponse = await GetHttpResponseMessageWithRetryAsync(url, "channels list");
            if(httpResponse is null)
            {
                continue;
            }

            var json = await httpResponse.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<YouTubeChannelsResponse>(json);

            if (result is not null)
            {
                allResults.Items.AddRange(result.Items);
            }
        }

        return allResults;
    }

    private async Task<HttpResponseMessage?> GetHttpResponseMessageWithRetryAsync(string url, string requestMethodName)
    {
        var maxRetry = 3;
        HttpResponseMessage? response = null;

        // 「初回 + 最大3回のリトライ」＝ 最大4回試行
        for (var tryCount = 0; tryCount <= maxRetry; tryCount++)
        {
            response = await _httpClient.GetAsync(url);

            // 503 かつ リトライ可能回数が残っている場合のみ待機
            if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                if(tryCount == maxRetry)
                {
                    ShowLog($"{requestMethodName} 503エラー。 これ以上 Retry 不可のため break");
                    break;
                }

                // 最大で 2 + 4 + 8 = 14 秒待機
                var delaySeconds = (int)Math.Pow(2, tryCount + 1);
                ShowLog($"{requestMethodName} 503エラー。{delaySeconds}秒後にリトライ");

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                continue;
            }

            if(!response.IsSuccessStatusCode)
            {
                ShowLog($"{requestMethodName} {response.StatusCode}エラー。 リクエスト失敗");
            }

            break;
        }

        return response;
    }

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

