using Microsoft.Extensions.Options;
using MyApi.Config;
using MyApi.DTOs;
using MyApi.Interfaces;
using MyApi.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Runtime;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using static System.Net.WebRequestMethods;

namespace MyApi.Services;

public class YouTubeService : IYouTubeService
{
    private readonly HttpClient _httpClient;
    private readonly YouTubeApiSettings _settings;
    private static readonly Dictionary<string, string> _cache = new();
    private static readonly HttpClientHandler _shortsHandler = new()
    {
        AllowAutoRedirect = false // ← リダイレクトを自動で追わない
    };
    private static readonly HttpClient _shortsClient = new(_shortsHandler);

    public YouTubeService(HttpClient httpClient, IOptions<YouTubeApiSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<string> SearchYouTubeVideosAsync(string keyword, string? pageToken)
    {
        var cacheKey = $"streams:{keyword}:{pageToken}";

        //今は開発用でapiトークンあんまり使わないようにキャッシュしてるけど、
        //本番環境ではキャッシュしないほうがいいかも
        if (_cache.TryGetValue(cacheKey, out var cached))
        {
            return cached; // キャッシュから返す
        }

        var url =
            "https://www.googleapis.com/youtube/v3/search" +
            $"?part=snippet" +
            $"&type=video" +
            $"&eventType=live" +
            $"&regionCode=JP" +
            $"&relevanceLanguage=ja" +
            $"&maxResults=10" +
            $"&q={Uri.EscapeDataString(keyword)}" +
            $"&key={_settings.ApiKey}";

        if(!string.IsNullOrEmpty(pageToken))
        {
            url += $"&pageToken={pageToken}";
        }

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"YouTube API error: {response.StatusCode}");
        }

        var json = await response.Content.ReadAsStringAsync();

        _cache[cacheKey] = json; // キャッシュに保存

        return json;
    }

    public async Task<string> GetShortsAsync(string keyword, string period, string? pageToken)
    {
        //search.list → YouTubeSearchResponseに変換
        var ytsr = await SearchVideoIdsAsync(keyword, period, pageToken);

        //リダイレクトを用いてshort動画を判定
        var shortVideos = await GetShortVideosAsync(ytsr);

        //json形式に再変換
        var finalJson = JsonSerializer.Serialize(shortVideos);
        return finalJson;
    }

    private async Task<YouTubeSearchResponse> SearchVideoIdsAsync(string keyword, string period, string? pageToken)
    {
        string? json = null;

        var cacheKey = $"streams:{keyword}:{period}:{pageToken}";
        if (_cache.TryGetValue(cacheKey, out var cached))
        {
            json = cached; // キャッシュから返す
        }

        if (json is null)
        {
            var url = $"https://www.googleapis.com/youtube/v3/search" +
                      $"?part=snippet" +
                      $"&type=video" +
                      $"&regionCode=JP" +
                      $"&relevanceLanguage=ja" +
                      $"&maxResults=50" +
                      $"&videoDuration=short" + // 4分未満に絞る
                      $"&q={Uri.EscapeDataString(keyword)}" +
                      $"&key={_settings.ApiKey}";

            if (period != SearchPeriod.All)
            {
                url += $"&publishedAfter={SearchPeriodHelper.GetStartDate(period)!.Value:O}";
            }

            if (!string.IsNullOrEmpty(pageToken))
            {
                url += $"&pageToken={pageToken}";
            }

            json = await _httpClient.GetStringAsync(url);
        }

        _cache[cacheKey] = json;

        var result = JsonSerializer.Deserialize<YouTubeSearchResponse>(json);
 
        if(result is null)
        {
            return new YouTubeSearchResponse();
        }

        return result;
    }

    private async Task<YouTubeSearchResponse> GetShortVideosAsync(YouTubeSearchResponse ytsr)
    {
        //並列処理で全動画を同時に判定
        var tasks = ytsr.Items.Select(async item =>
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

    //private async Task<List<VideoDetail>> GetVideoDetailsAsync(List<string> videoIds)
    //{
    //    string? json = null;

    //    var cacheKey = string.Join("&", videoIds);
    //    if (_cache.TryGetValue(cacheKey, out var cached))
    //    {
    //        json = cached; // キャッシュから返す
    //    }

    //    if (json is null)
    //    {

    //        if (videoIds.Count == 0)
    //        {
    //            return new List<VideoDetail>();
    //        }

    //        var ids = string.Join(",", videoIds);

    //        var url = $"https://www.googleapis.com/youtube/v3/videos" +
    //                  $"?part=snippet,contentDetails" +
    //                  $"&id={ids}" +
    //                  $"&key={_settings.ApiKey}";

    //        json = await _httpClient.GetStringAsync(url);
    //    }
    //    _cache[cacheKey] = json;

    //    var result = JsonSerializer.Deserialize<DetailSearchResponse>(json);/*, new JsonSerializerOptions*/
    //    //{
    //    //    PropertyNameCaseInsensitive = true
    //    //});
    //    if(result is null)
    //    {
    //        return new List<VideoDetail>();
    //    }

    //    return result.Items;
    //}

    //private static bool IsShort(VideoDetail video)
    //{
    //    if (!TryParseDuration(video.ContentDetails.Duration, out var duration))
    //    {
    //        return false;
    //    }

    //    if (duration.TotalSeconds > YouTubeShortSeconds)
    //    {
    //        return false;
    //    }

    //    var thumb = video.Snippet.Thumbnails.High
    //                ?? video.Snippet.Thumbnails.Medium
    //                ?? video.Snippet.Thumbnails.Default;

    //    if (thumb is null)
    //    {
    //        return false;
    //    }

    //    if (thumb.Height <= thumb.Width)
    //    {
    //        return false;
    //    }

    //    return true;
    //}

    //private static bool TryParseDuration(string isoDuration, out TimeSpan result)
    //{
    //    try
    //    {
    //        result = XmlConvert.ToTimeSpan(isoDuration);
    //        return true;
    //    }
    //    catch
    //    {
    //        result = TimeSpan.Zero;
    //        return false;
    //    }
    //}

    //private YouTubeSearchResponse ConvertToSearchResponse(List<VideoDetail> shortInfos)
    //{
    //    var ytsrItems = new List<SearchItem>();
    //    foreach (var shortInfo in shortInfos)
    //    {
    //        var searchItem = new SearchItem
    //        {
    //            Id = new SearchId { VideoId = shortInfo.Id },
    //            Snippet = shortInfo.Snippet
    //        };
    //        ytsrItems.Add(searchItem);
    //    }

    //    var response = new YouTubeSearchResponse
    //    {
    //        Items = ytsrItems
    //    };

    //    return response;
    //}

    //public async Task<List<YouTubeVideoDetailDto>> SearchVideosWithDetailsAsync(string keyword)
    //{
    //    // 1. 検索 API
    //    var searchUrl =
    //        "https://www.googleapis.com/youtube/v3/search" +
    //        $"?part=snippet&type=video&maxResults=10&q={Uri.EscapeDataString(keyword)}&key={_settings.ApiKey}";

    //    var searchResponse = await _httpClient.GetStringAsync(searchUrl);
    //    var searchJson = JObject.Parse(searchResponse);

    //    var videoIds = searchJson["items"]?
    //        .Select(i => (string?)i["id"]?["videoId"])
    //        .ToList();
    //    if(videoIds is null)
    //    {
    //        return new List<YouTubeVideoDetailDto>();
    //    }

    //    if (!videoIds.Any())
    //    {
    //        return new List<YouTubeVideoDetailDto>();
    //    }

    //    // 2. 詳細 API
    //    var detailsUrl =
    //        "https://www.googleapis.com/youtube/v3/videos" +
    //        $"?part=snippet,statistics,contentDetails&id={string.Join(",", videoIds)}&key={_settings.ApiKey}";

    //    var detailsResponse = await _httpClient.GetStringAsync(detailsUrl);
    //    var detailsJson = JObject.Parse(detailsResponse);

    //    // 3. DTO に整形
    //    var result = new List<YouTubeVideoDetailDto>();
    //    var items = detailsJson["items"];
    //    if(items is null)
    //    {
    //        return result;
    //    }

    //    foreach (var item in items)
    //    {
    //        result.Add(new YouTubeVideoDetailDto
    //        {
    //            VideoId = (string?)item["id"]?? "",
    //            Title = (string?)item["snippet"]?["title"]?? "",
    //            ThumbnailUrl = (string?)item["snippet"]?["thumbnails"]?["high"]?["url"]?? "",
    //            ChannelTitle = (string?)item["snippet"]?["channelTitle"]?? "",
    //            PublishedAt = (string?)item["snippet"]?["publishedAt"]?? "",
    //            ViewCount = (long?)item["statistics"]?["viewCount"]?? 0,
    //            LikeCount = (long?)item["statistics"]?["likeCount"]?? 0,
    //            CommentCount = (long?)item["statistics"]?["commentCount"]?? 0,
    //            Duration = (string?)item["contentDetails"]?["duration"]?? ""
    //        });
    //    }

    //    return result;
    //}
}

//public class LowerCaseNamingPolicy : JsonNamingPolicy
//{
//    public override string ConvertName(string name)
//        => name.ToLower();
//}

