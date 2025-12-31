using Microsoft.Extensions.Options;
using MyApi.Config;
using MyApi.Interfaces;
using MyApi.Models;
using System.Net.Http;
using System.Text.Json;

namespace MyApi.Services;

public class TwitchService : ITwitchService
{
    private readonly HttpClient _httpClient;
    private readonly TwitchApiSettings _settings;
    private static string? _cachedToken;
    private static DateTime _tokenExpiresAt;

    // ★ YouTube と同じ構造でキャッシュを用意
    private static readonly Dictionary<string, string> _cache = new();

    public TwitchService(HttpClient httpClient, IOptions<TwitchApiSettings> settings)
    {
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


    // ★ Twitch の検索（配信・チャンネル検索）
    public async Task<string> SearchTwitchVideosAsync(string keyword)
    {
        // キャッシュチェック
        if (_cache.TryGetValue(keyword, out var cached))
        {
            return cached;
        }

        var token = await GetAccessTokenAsync();

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://api.twitch.tv/helix/search/channels?query={keyword}"
        );

        request.Headers.Add("Client-ID", _settings.ClientId);
        request.Headers.Add("Authorization", $"Bearer {token}");

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Twitch API error: {response.StatusCode}");
        }

        var json = await response.Content.ReadAsStringAsync();

        // キャッシュ保存
        _cache[keyword] = json;

        return json;
    }

    // ★ Twitch のカテゴリ検索（ゲームカテゴリ）
    public async Task<string> SearchCategoriesAsync(string keyword)
    {
        // キャッシュチェック
        if (_cache.TryGetValue($"cat:{keyword}", out var cached))
        {
            return cached;
        }

        var token = await GetAccessTokenAsync();

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://api.twitch.tv/helix/search/categories?query={keyword}"
        );

        request.Headers.Add("Client-ID", _settings.ClientId);
        request.Headers.Add("Authorization", $"Bearer {token}");

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Twitch API error: {response.StatusCode}");
        }

        var json = await response.Content.ReadAsStringAsync();

        // ★ カテゴリ検索は prefix を付けてキャッシュ
        _cache[$"cat:{keyword}"] = json;

        return json;
    }


    //// ★ Twitch の配信一覧取得（カテゴリID → Streams API）
    //public async Task<string> GetStreamsByCategoryAsync(string categoryId)
    //{
    //    // キャッシュチェック
    //    if (_cache.TryGetValue($"streams:{categoryId}", out var cached))
    //    {
    //        return cached;
    //    }

    //    var token = await GetAccessTokenAsync();

    //    var request = new HttpRequestMessage(
    //        HttpMethod.Get,
    //        $"https://api.twitch.tv/helix/streams?game_id={categoryId}&language=ja"
    //    );

    //    request.Headers.Add("Client-ID", _settings.ClientId);
    //    request.Headers.Add("Authorization", $"Bearer {token}");

    //    var response = await _httpClient.SendAsync(request);

    //    if (!response.IsSuccessStatusCode)
    //    {
    //        throw new Exception($"Twitch API error: {response.StatusCode}");
    //    }

    //    var json = await response.Content.ReadAsStringAsync();

    //    // ★ Streams もキャッシュしておく
    //    _cache[$"streams:{categoryId}"] = json;

    //    return json;
    //}

    public async Task<string> GetStreamsByCategoryAsync(string categoryId, string? cursor)
    {
        var cacheKey = $"streams:{categoryId}:{cursor}";

        //今は開発用でapiトークンあんまり使わないようにキャッシュしてるけど、
        //本番環境ではキャッシュしないほうがいいかも
        if (_cache.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        var token = await GetAccessTokenAsync();

        var url = $"https://api.twitch.tv/helix/streams?game_id={categoryId}&language=ja&first=10";

        if (!string.IsNullOrEmpty(cursor))
        {
            url += $"&after={cursor}";
        }

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Client-ID", _settings.ClientId);
        request.Headers.Add("Authorization", $"Bearer {token}");

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Twitch API error: {response.StatusCode}");
        }

        var json = await response.Content.ReadAsStringAsync();

        _cache[cacheKey] = json;

        return json;
    }

    public async Task<string> GetClipsByCategoryAsync(string categoryId, string period, string? cursor)
    {
        var cacheKey = $"clips:{categoryId}:{cursor}";

        //今は開発用でapiトークンあんまり使わないようにキャッシュしてるけど、
        //本番環境ではキャッシュしないほうがいいかも
        if (_cache.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        var token = await GetAccessTokenAsync();

        var url = $"https://api.twitch.tv/helix/clips?game_id={categoryId}&language=ja&first=10";

        if (period != SearchPeriod.All)
        {
            url += $"&started_at={SearchPeriodHelper.GetStartDate(period)!.Value:O}";
        }

        if (!string.IsNullOrEmpty(cursor))
        {
            url += $"&after={cursor}";
        }

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Client-ID", _settings.ClientId);
        request.Headers.Add("Authorization", $"Bearer {token}");

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Twitch API error: {response.StatusCode}");
        }

        var json = await response.Content.ReadAsStringAsync();

        _cache[cacheKey] = json;

        return json;
    }
}
