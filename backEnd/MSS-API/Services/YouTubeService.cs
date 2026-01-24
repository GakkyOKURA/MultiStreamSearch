using Microsoft.Extensions.Options;
using MyApi.Config;
using MyApi.Interfaces;
using MyApi.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MyApi.Services;

public class YouTubeService : IYouTubeService
{
    private readonly RedisCacheService _cache;
    private readonly HttpClient _httpClient;
    private readonly YouTubeApiSettings _settings;
    private static readonly HttpClientHandler _shortsHandler = new()
    {
        AllowAutoRedirect = false // ← リダイレクトを自動で追わない
    };
    private static readonly HttpClient _shortsClient = new(_shortsHandler);

    public YouTubeService(RedisCacheService cache, HttpClient httpClient, IOptions<YouTubeApiSettings> settings)
    {
        _cache = cache;
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    // ユーザーからのアクセス
    public async Task<YouTubeSearchResult> SearchYouTubeShortsAsync(string keyword)
    {
        var cacheKey = CacheKeyHelper.GetCacheKey(CacheKeyHelper.VideoType.YouTubeShort, keyword);

        // Redis から取得
        var cached = await _cache.GetStringAsync(cacheKey);
        if (cached is null)
        {
            // キャッシュが無い場合は API を叩かず、エラーを返す
            throw new Exception("Cache not ready. Please try again later.");
        }

        var result = JsonSerializer.Deserialize<YouTubeSearchResult>(cached);
        return result ?? new YouTubeSearchResult();
    }

    // バックエンドからのアクセス
    public async Task<YouTubeSearchResult> FetchYouTubeShortsAsync(string keyword)
    {
        //search.list → YouTubeSearchResponseに変換
        var response = await SearchVideoIdsAsync(keyword);

        //リダイレクトを用いてshort動画を判定
        var shortVideos = await GetShortVideosAsync(response);

        // 日付順に並び替える
        var orderedShorts = GetOrderedShortsByDateDescending(shortVideos);

        //日本語を前に
        var japaneseSortedShort = SortJapaneseFirst(orderedShorts);

        // YouTubeSearchResult に整形し、不要な情報を削除
        var result = GetResult(japaneseSortedShort);
        return result;
    }

    private List<YouTubeSearchItemRaw> GetOrderedShortsByDateDescending(YouTubeSearchResponse response)
    {
        return response.Items
            .OrderByDescending(v => v.Snippet.PublishedAt)
            .ToList();
    }

    private YouTubeSearchResult GetResult(List<YouTubeSearchItemRaw> pItems)
    {
        var items = pItems
            .Select(shortInfo => new YouTubeSearchItemDto
            {
                Id = new YouTubeSearchItemIdDto
                {
                    VideoId = shortInfo.Id.VideoId,
                    ChannelId = shortInfo.Id.ChannelId,
                },
                Snippet = new YouTubeSnippetDto
                {
                    ChannelId = shortInfo.Snippet.ChannelId,
                    Title = shortInfo.Snippet.Title,
                    Description = shortInfo.Snippet.Description,
                    Thumbnails = new YouTubeThumbnailsDto
                    {
                        Medium = new YouTubeThumbnailDto
                        {
                            Url = shortInfo.Snippet.Thumbnails.Medium.Url,
                            Width = shortInfo.Snippet.Thumbnails.Medium.Width,
                            Height = shortInfo.Snippet.Thumbnails.Medium.Height
                        }
                    },
                    ChannelTitle = shortInfo.Snippet.ChannelTitle,
                }
            })
            .ToList();

        return new YouTubeSearchResult
        {
            Items = items
        };
    }

    private async Task<YouTubeSearchResponse> SearchVideoIdsAsync(string keyword)
    {
        var url = $"https://www.googleapis.com/youtube/v3/search" +
                  $"?part=snippet" +
                  $"&type=video" +
                  $"&regionCode=JP" +
                  $"&relevanceLanguage=ja" +
                  $"&maxResults=50" +
                  $"&order=date" +
                  $"&videoDuration=short" + // 4分未満に絞る
                  $"&q={Uri.EscapeDataString(keyword)}" +
                  $"&key={_settings.ApiKey}";

        //TODO:期間を指定した検索
        // YouTube のクォータ制限が緩和されたら実装
        url += $"&publishedAfter={SearchPeriodHelper.GetStartDate(SearchPeriod.Day)!.Value:O}";

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"YouTube API error: {response.StatusCode}");
        }

        var json = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<YouTubeSearchResponse>(json);
        if (result is null)
        {
            return new YouTubeSearchResponse();
        }

        return result;
    }

    private async Task<YouTubeSearchResponse> GetShortVideosAsync(YouTubeSearchResponse respose)
    {
        //並列処理で全動画を同時に判定
        var tasks = respose.Items.Select(async item =>
        {
            var isShort = await IsShortVideoAsync(item.Id.VideoId);
            return (item, isShort);
        });

        var results = await Task.WhenAll(tasks);

        return new YouTubeSearchResponse
        {
            Items = results
                .Where(r => r.isShort)
                .Select(r => r.item)
                .ToList()
        };
    }

    private async Task<bool> IsShortVideoAsync(string videoId)
    {
        var url = $"https://www.youtube.com/shorts/{videoId}";

        // HEAD で十分（本文不要）
        using var request = new HttpRequestMessage(HttpMethod.Head, url);

        var response = await _shortsClient.SendAsync(request);

        // リダイレクトされる場合は 301 / 302 / 303 / 307 / 308 のいずれか
        if ((int)response.StatusCode >= 300 && (int)response.StatusCode < 400)
        {
            // Location が /watch?v=... なら通常動画
            var location = response.Headers.Location?.ToString() ?? "";

            if (location.Contains("/watch"))
            {
                return false; // 通常動画
            }
        }

        return true; // リダイレクトなし → Shorts
    }

    // ユーザーからのアクセス
    public async Task<YouTubeSearchResult> SearchYouTubeLiveStreamsAsync(string keyword)
    {
        var cacheKey = CacheKeyHelper.GetCacheKey(CacheKeyHelper.VideoType.YouTubeLiveStream, keyword);

        // Redis から取得
        var cached = await _cache.GetStringAsync(cacheKey);
        if (cached is null)
        {
            // キャッシュが無い場合は API を叩かず、エラーを返す
            throw new Exception("Cache not ready. Please try again later.");
        }

        var result = JsonSerializer.Deserialize<YouTubeSearchResult>(cached);
        return result ?? new YouTubeSearchResult();
    }

    //バックエンドからのアクセス
    public async Task<YouTubeSearchResult> FetchYouTubeLiveStreamsAsync(string keyword)
    {
        var url =
            "https://www.googleapis.com/youtube/v3/search" +
            $"?part=snippet" +
            $"&type=video" +
            $"&eventType=live" +
            $"&regionCode=JP" +
            $"&relevanceLanguage=ja" +
            $"&maxResults=50" +
            $"&q={Uri.EscapeDataString(keyword)}" +
            $"&key={_settings.ApiKey}";

        var httpResponse = await _httpClient.GetAsync(url);
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception($"YouTube API error: {httpResponse.StatusCode}");
        }

        var json = await httpResponse.Content.ReadAsStringAsync();
        var response = JsonSerializer.Deserialize<YouTubeSearchResponse>(json);
        if(response is null)
        {
            return new YouTubeSearchResult();
        }

        var sortedResponse = SortJapaneseFirst(response.Items);
        var result = GetResult(sortedResponse);
        return result;
    }

    //チャンネル名とタイトルどちらかに日本語が入っていれば日本語の動画判定とする
    //クォータに余裕があれば search video で判定する
    private List<YouTubeSearchItemRaw> SortJapaneseFirst(List<YouTubeSearchItemRaw> items)
    {
        return items
            .OrderByDescending(v =>
                HasJapanese(v.Snippet.Title) || HasJapanese(v.Snippet.ChannelTitle))
            .ToList();
    }

    private static bool HasJapanese(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;

        // ひらがな・カタカナ・漢字
        return Regex.IsMatch(text, @"[\u3040-\u30FF\u4E00-\u9FFF]");
    }

}

