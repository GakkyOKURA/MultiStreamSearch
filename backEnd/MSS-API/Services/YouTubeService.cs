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
using MyApi.Models.YouTubeShortSearchHelper;

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
            _cache[cacheKey] = json;
        }

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
}

