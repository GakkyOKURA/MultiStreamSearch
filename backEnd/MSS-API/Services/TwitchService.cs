using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using MyApi.Config;
using MyApi.Interfaces;
using MyApi.Models;
using System.Text.Json;

namespace MyApi.Services;

public class TwitchService : ITwitchService
{
    private readonly RedisCacheService _cache;
    private readonly HttpClient _httpClient;
    private readonly TwitchApiSettings _settings;
    private static string? _cachedToken;
    private static DateTime _tokenExpiresAt;


    public TwitchService(RedisCacheService cache, HttpClient httpClient, IOptions<TwitchApiSettings> settings)
    {
        _cache = cache;
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    // ★ アプリ用トークンを取得（Client Credentials Flow）
    public async Task<string> GetAccessTokenAsync()
    {
        // ★ 有効なトークンがあれば再利用
        if (_cachedToken != null && DateTime.UtcNow < _tokenExpiresAt)
        {
            return _cachedToken;
        }

        var url =
            "https://id.twitch.tv/oauth2/token" +
            $"?client_id={_settings.ClientId}" +
            $"&client_secret={_settings.ClientSecret}" +
            "&grant_type=client_credentials";

        var res = await _httpClient.PostAsync(url, null);
        var json = await res.Content.ReadAsStringAsync();

        var doc = JsonDocument.Parse(json).RootElement;

        _cachedToken = doc.GetProperty("access_token").GetString();
        var expiresIn = doc.GetProperty("expires_in").GetInt32(); // 秒数

        // ★ 有効期限を保存（現在時刻 + expires_in）
        _tokenExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn - 60);
        // -60(秒)は余裕を持たせるため

        return _cachedToken!;
    }

    // ユーザーからのアクセス
    public async Task<TwitchStreamSearchResult> SearchTwitchStreamsAsync(string gameId)
    {
        var cacheKey = CacheKeyHelper.GetCacheKey(CacheKeyHelper.VideoType.TwitchStream, gameId);

        var cached = await _cache.GetStringAsync(cacheKey);
        if(cached is null)
        {
            throw new Exception("Cache not ready. Please try again later.");
        }

        var result = JsonSerializer.Deserialize<TwitchStreamSearchResult>(cached);
        return result ?? new TwitchStreamSearchResult();
    }

    // バックエンドからのアクセス
    public async Task<TwitchStreamSearchResult> FetchTwitchStreamsAsync(string gameId)
    {
        var token = await GetAccessTokenAsync();

        var url = $"https://api.twitch.tv/helix/streams?game_id={gameId}&language=ja&first=50";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Client-ID", _settings.ClientId);
        request.Headers.Add("Authorization", $"Bearer {token}");

        var httpResponse = await _httpClient.SendAsync(request);
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception($"Twitch API error: {httpResponse.StatusCode}");
        }

        var json = await httpResponse.Content.ReadAsStringAsync();
        var response = JsonSerializer.Deserialize<TwitchStreamSearchResponse>(json);
        if(response is null)
        {
            return new TwitchStreamSearchResult();
        }

        var result = GetStreamResult(response);
        return result;
    }

    private TwitchStreamSearchResult GetStreamResult(TwitchStreamSearchResponse response)
    {
        var dataDto = response.Data
            .Select(v => new TwitchStreamSearchDto
            {
                Id = v.Id,
                UserId = v.UserId,
                UserLogin = v.UserLogin,
                UserName = v.UserName,
                GameId = v.GameId,
                GameName = v.GameName,
                Title = v.Title,
                ThumbnailUrl = v.ThumbnailUrl
            })
            .ToList();

        return new TwitchStreamSearchResult
        {
            Data = dataDto
        };
    }

    // ユーザーからのアクセス
    public async Task<TwitchClipSearchResult> SearchTwitchClipsAsync(string gameId)
    {
        var cacheKey = CacheKeyHelper.GetCacheKey(CacheKeyHelper.VideoType.TwitchClip, gameId);

        var cached = await _cache.GetStringAsync(cacheKey);
        if(cached is null )
        {
            throw new Exception("Cache not ready. Please try again later.");
        }

        var result = JsonSerializer.Deserialize<TwitchClipSearchResult>(cached);
        return result ?? new TwitchClipSearchResult();
    }

    // バックエンドからのアクセス
    public async Task<TwitchClipSearchResult> FetchTwitchClipsAsync(string gameId)
    {
        // clip 検索は言語でのフィルタリングができないから
        // 最大1000件取得し、日本語に絞る
        var respose = await FetchJapaneseTwitchClipsAsync(gameId);

        // TwitchClipSearchResult に整形
        var result = GetClipResult(respose);
        return result;
    }

    private async Task<TwitchClipSearchResponse> FetchJapaneseTwitchClipsAsync(string gameId)
    {
        var token = await GetAccessTokenAsync();
        
        var baseUrl = $"https://api.twitch.tv/helix/clips?game_id={gameId}" +
            $"&first=100";

        //TODO:期間を指定した検索
        // YouTube のクォータ制限が緩和されたら実装
        baseUrl += $"&started_at={SearchPeriodHelper.GetStartDate(SearchPeriod.Day)!.Value:O}";

        var maxSearchRoop = 10;
        var pagination = new TwitchClipPaginationRaw();
        var data = new List<TwitchClipSearchRaw>();
        for (var i = 0; i < maxSearchRoop; i++)
        {
            var url = baseUrl;

            if(!string.IsNullOrEmpty(pagination.Cursor))
            {
                url += $"&after={pagination.Cursor}";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Client-ID", _settings.ClientId);
            request.Headers.Add("Authorization", $"Bearer {token}");

            var httpResponse = await _httpClient.SendAsync(request);
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Twitch API error: {httpResponse.StatusCode}");
            }

            var json = await httpResponse.Content.ReadAsStringAsync();

            // TwitchClipSearchResponse 型に変換
            var response = JsonSerializer.Deserialize<TwitchClipSearchResponse>(json);
            if (response is null)
            {
                return new TwitchClipSearchResponse();
            }

            pagination = response.Pagination;

            var japaneseClip = GetJapaneseClip(response);
            data.AddRange(japaneseClip);

            //Twitch API の仕様上、 page1 の末尾と page2 の先頭に
            //同じクリップが入ることがある。それを防ぐ。
            data = data
                .DistinctBy(v => v.Id)
                .ToList();

            // data が 50 を超えた場合は cursor が残ってても break
            // cursor が無くなった = 最後まで検索された場合は break
            if (data.Count > 50 || string.IsNullOrEmpty(pagination.Cursor))
            {
                data = data
                    .OrderByDescending(v => v.CreatedAt)// 最新の動画を前に
                    .Take(50)
                    .ToList();
                break;
            }
        }

        return new TwitchClipSearchResponse
        {
            Data = data,
            Pagination = pagination
        };
    }

    private List<TwitchClipSearchRaw> GetJapaneseClip(TwitchClipSearchResponse response)
    {
        return response.Data
            .Where(v => v.Language == "ja")
            .ToList();
    }

    private TwitchClipSearchResult GetClipResult(TwitchClipSearchResponse response)
    {
        var dataDto = response.Data
            .Select(v => new TwitchClipSearchDto
            {
                Id = v.Id,
                Url = v.Url,
                EmbedUrl = v.EmbedUrl,
                BroadcasterId = v.BroadcasterId,
                BroadcasterName = v.BroadcasterName,
                VideoId = v.VideoId,
                Title = v.Title,
                ThumbnailUrl = v.ThumbnailUrl
            })
            .ToList();

        return new TwitchClipSearchResult
        {
            Data = dataDto
        };
    }


    // ★ Twitch のカテゴリ検索（ゲームカテゴリ）
    // 将来的にゲームが自由に検索できるようになれば使用。
    // 現在は検索できるゲームを手動で指定

    //private static readonly Dictionary<string, string> _cache = new();

    //public async Task<string> SearchCategoriesAsync(string keyword)
    //{
    //    // キャッシュチェック
    //    if (_cache.TryGetValue($"cat:{keyword}", out var cached))
    //    {
    //        return cached;
    //    }

    //    var token = await GetAccessTokenAsync();

    //    var request = new HttpRequestMessage(
    //        HttpMethod.Get,
    //        $"https://api.twitch.tv/helix/search/categories?query={keyword}"
    //    );

    //    request.Headers.Add("Client-ID", _settings.ClientId);
    //    request.Headers.Add("Authorization", $"Bearer {token}");

    //    var response = await _httpClient.SendAsync(request);

    //    if (!response.IsSuccessStatusCode)
    //    {
    //        throw new Exception($"Twitch API error: {response.StatusCode}");
    //    }

    //    var json = await response.Content.ReadAsStringAsync();

    //    // ★ カテゴリ検索は prefix を付けてキャッシュ
    //    _cache[$"cat:{keyword}"] = json;

    //    return json;
    //}
}
