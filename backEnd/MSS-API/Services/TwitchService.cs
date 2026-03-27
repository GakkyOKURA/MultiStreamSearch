using Microsoft.Extensions.Options;
using MyApi.Config;
using MyApi.DTOs;
using MyApi.Interfaces;
using MyApi.Models;
using MyApi.Raws;
using System.Text.Json;

namespace MyApi.Services;

public class TwitchService : ITwitchService
{
    private readonly IRedisCacheService _cache;
    private readonly HttpClient _httpClient;
    private readonly TwitchApiSettings _settings;
    private static string? _cachedToken;
    private static DateTime _tokenExpiresAt;
    private readonly ILogger<TwitchService> _logger;

    public TwitchService(
        IRedisCacheService cache,
        HttpClient httpClient,
        IOptions<TwitchApiSettings> settings,
        ILogger<TwitchService> logger)
    {
        _cache = cache;
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    // アプリ用トークンを取得（Client Credentials Flow）
    private async Task<string> GetAccessTokenAsync()
    {
        // 有効なトークンがあれば再利用
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

    public async Task<VideoDataResponse> SearchTwitchStreamsAsync()
    {
        var cacheKey = CacheKeyHelper.GetCacheKey(CacheKeyHelper.VideoType.TwitchStream);
        return await _cache.GetAsync<VideoDataResponse>(cacheKey) ?? new();
    }

    public async Task<VideoDataResponse> FetchTwitchStreamsAsync()
    {
        var result = new VideoDataResponse();
        TwitchStreamPaginationRaw? oldPagination = null;
        // 結果を 100 個取得できるまでループ
        // while が適切だが無限ループが怖いので for にしとく
        for (var i = 0; i < 5; i++)
        {
            var streamResponse = await GetVtuberTwitchStreamsAsync(oldPagination);
            
            oldPagination = streamResponse.Pagination;

            var channelResponse = await GetChannelInformationAsync(streamResponse.Data);
            var dto = ToDTO(streamResponse.Data, channelResponse.Data);
            var individual = dto.Items
                .Where(v => !IndividualChecker.IsCompany(v.ChannelDescription))
                .ToList();

            result.Items.AddRange(individual);
            if(result.Items.Count >= 100 || string.IsNullOrEmpty(oldPagination.Cursor))
            {
                result.Items = result.Items
                    .Take(100)
                    .ToList();
                break;
            }
        }

        return result;
    }

    private async Task<TwitchStreamSearchResponse> GetVtuberTwitchStreamsAsync(TwitchStreamPaginationRaw? oldPaination = null)
    {
        var token = await GetAccessTokenAsync();

        var baseUrl =
            "https://api.twitch.tv/helix/streams" +
            "?language=ja" +
            "&first=100";

        var maxSearchRoop = 20;
        var pagination = oldPaination ?? new TwitchStreamPaginationRaw();
        var data = new List<TwitchStreamSearchRaw>();
        for (var i = 0; i < maxSearchRoop; i++)
        {
            var url = baseUrl;

            if (!string.IsNullOrEmpty(pagination.Cursor))
            {
                url += $"&after={pagination.Cursor}";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Client-ID", _settings.ClientId);
            request.Headers.Add("Authorization", $"Bearer {token}");

            // 最大 20 回ループ。 許容範囲
            var httpResponse = await GetHttpRequestWithRetryAsync(request, "get streams");
            if(httpResponse is null)
            {
                continue;
            }

            var json = await httpResponse.Content.ReadAsStringAsync();

            // TwitchClipSearchResponse 型に変換
            var response = JsonSerializer.Deserialize<TwitchStreamSearchResponse>(json);
            if (response is null)
            {
                return new();
            }

            pagination = response.Pagination;

            var vtuberStream = FilterVtuberStream(response);
            data.AddRange(vtuberStream);

            //Twitch API の仕様上、 page1 の末尾と page2 の先頭に
            //同じ動画が入ることがある。それを防ぐ。
            data = data
                .DistinctBy(v => v.Id)
                .ToList();

            // data が 100 を超えた場合は cursor が残ってても break
            // cursor が無くなった = 最後まで検索された場合は break
            // 重複でカウントが加算されるのはよくないので DistinctBy した後にカウントの確認
            if (data.Count >= 100 || string.IsNullOrEmpty(pagination.Cursor))
            {
                break;
            }
        }

        return new TwitchStreamSearchResponse
        {
            Data = data,
            Pagination = pagination
        };
    }

    private List<TwitchStreamSearchRaw> FilterVtuberStream(TwitchStreamSearchResponse response )
    {
        return response.Data
            .Where(v => v.Tags is not null)
            .Where(v => IsVtuber(v.Tags))
            .ToList();
    }

    private bool IsVtuber(List<string> tags)
    {
        // いずれかのタグの中に "vtuber" という単語が含まれているか
        return tags.Any(t => t.Contains("vtuber", StringComparison.OrdinalIgnoreCase));
    }

    private async Task<TwitchUserResponse> GetChannelInformationAsync(List<TwitchStreamSearchRaw> data)
    {
        if (data.Count == 0)
        {
            return new TwitchUserResponse();
        }

        var token = await GetAccessTokenAsync();
        var allUsers = new List<TwitchUserRaw>(); // 全結果を格納するリスト

        // 50個ずつのチャンクに分けて処理
        foreach (var chunk in data.Chunk(50))
        {
            var idQuery = string.Join("&id=", chunk.Select(s => s.UserId));
            var url = $"https://api.twitch.tv/helix/users?id={idQuery}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Client-ID", _settings.ClientId);
            request.Headers.Add("Authorization", $"Bearer {token}");

            var httpResponse = await GetHttpRequestWithRetryAsync(request, "get users");
            if(httpResponse is null)
            {
                continue;
            }

            var json = await httpResponse.Content.ReadAsStringAsync();
            var userResponse = JsonSerializer.Deserialize<TwitchUserResponse>(json);

            if (userResponse is not null)
            {
                allUsers.AddRange(userResponse.Data);
            }
        }

        return new TwitchUserResponse { Data = allUsers };
    }

    private async Task<HttpResponseMessage?> GetHttpRequestWithRetryAsync(
        HttpRequestMessage request,
        string requestMethodName)
    {
        var maxRetry = 3;
        HttpResponseMessage? response = null;

        for(var tryCount = 0; tryCount < maxRetry; tryCount++)
        {
            response = await _httpClient.SendAsync(request);
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

            if (!response.IsSuccessStatusCode)
            {
                ShowLog($"{requestMethodName} {response.StatusCode}エラー。 リクエスト失敗");
            }

            break;
        }

        return response;
    }

    private VideoDataResponse ToDTO(List<TwitchStreamSearchRaw> sData, List<TwitchUserRaw> cData)
    {
        if(sData.Count == 0 || cData.Count == 0)
        {
            return new VideoDataResponse();
        }

        return new VideoDataResponse
        {
            Items = sData
                .Join(
                cData,
                s => s.UserId,
                u => u.Id,
                (s, u) => new VideoDataDTO // YouTubeと共通のDTOに変換
                {
                    VideoId = s.Id,
                    VideoTitle = s.Title,
                    ChannelId = s.UserLogin,//UserId,
                    ChannelName = s.UserName,
                    // Twitch の概要欄をセット
                    SearchDescription = string.Join(" ", s.Tags), // Twitchには動画単位の説明文がないのでタイトルを流用
                    ChannelDescription = u.Description,

                    // サムネイルURLの {width}x{height} を置換
                    SearchHighTumbnail = new VideoHighThumbnailDTO
                    {
                        Url = s.ThumbnailUrl.Replace("{width}", "1280").Replace("{height}", "720"),
                        Width = 1280,
                        Height = 720
                    },
                    ChannelHighThumbnail = new VideoHighThumbnailDTO
                    {
                        Url = u.ProfileImageUrl,
                        Width = 300,
                        Height = 300
                    },

                    // KeywordsをTwitchのデータから合成する
                    Keywords = string.Join(", ", new List<string>
                    {
                        s.GameName,
                        u.BroadcasterType,
                        string.Join(", ", s.Tags)
                    }
                    .Where(str => !string.IsNullOrEmpty(str))), // 空文字を除外

                    BannerExternalUrl = u.OfflineImageUrl,
                    TopicCategories = s.Tags, // YouTubeのGenresと同じ扱い
                    Platform = VIdeoPlatform.Twitch
                }).ToList()
        };
    }

    private void ShowLog(string message)
    {
        var time = DateTime.Now;
        _logger.LogInformation("\n{Time}{Message}\n", time, message);
    }
}
