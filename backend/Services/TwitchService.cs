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
    private readonly IHostEnvironment _environment;
    private readonly HttpClient _httpClient;
    private readonly TwitchApiSettings _settings;
    private readonly ILogger<TwitchService> _logger;

    public TwitchService(
        IRedisCacheService cache,
        IHostEnvironment hostEnvironment,
        HttpClient httpClient,
        IOptions<TwitchApiSettings> settings,
        ILogger<TwitchService> logger)
    {
        _cache = cache;
        _environment = hostEnvironment;
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    // アプリ用トークンを取得（Client Credentials Flow）
    private async Task<string> GetAccessTokenAsync()
    {
        // 有効なトークンがあれば再利用
        if (await _cache.GetStringAsync(CacheKeyHelper.GetTwitchTokenCacheKey()) is string existingToken)
        {
            return existingToken;
        }

        var url =
            "https://id.twitch.tv/oauth2/token" +
            $"?client_id={_settings.ClientId}" +
            $"&client_secret={_settings.ClientSecret}" +
            "&grant_type=client_credentials";

        using var res = await _httpClient.PostAsync(url, null);
        var json = await res.Content.ReadAsStringAsync();

        var doc = JsonDocument.Parse(json).RootElement;

        var token = doc.GetProperty("access_token").GetString();
        var expiresIn = doc.GetProperty("expires_in").GetInt32(); // 何秒で token が切れるかを把握

        if (token is null)
        {
            return "";
        }

        // ★ 有効期限を保存（現在時刻 + expires_in）
        // -60(秒)は余裕を持たせるため
        var tokenExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn - 60);
        var ttl = tokenExpiresAt - DateTime.UtcNow;

        await _cache.SetStringAsync(CacheKeyHelper.GetTwitchTokenCacheKey(), token, ttl);

        return token;
    }

    public async Task<VideoDataResponse> SearchTwitchStreamsAsync()
    {
        var cacheKey = CacheKeyHelper.GetCacheKey(VideoType.TwitchStream);
        return await _cache.GetAsync<VideoDataResponse>(cacheKey) ?? new();
    }

    public async Task<VideoDataResponse> FetchTwitchStreamsAsync()
    {
        if (_environment.IsDevelopment())
        {
            return CreateDataForDebug();
        }

        var result = new VideoDataResponse();
        TwitchStreamPaginationRaw? oldPagination = null;
        // 結果を 100 個取得できるまでループ
        // while が適切だが無限ループが怖いので for にしとく
        for (var i = 0; i < 5; i++)
        {
            // まずは配信を取得
            var streamResponse = await GetVtuberTwitchStreamsAsync(oldPagination);
            // pagination を更新
            oldPagination = streamResponse.Pagination;

            // 次にチャンネル情報を取得
            var channelResponse = await GetChannelInformationAsync(streamResponse.Data);

            // dto の形に整形
            var dto = ToDTO(streamResponse.Data, channelResponse.Data);

            result.Items.AddRange(dto.Items);
            // 100 個取得できた or pagination が切れたら beak
            if (result.Items.Count >= 100 || string.IsNullOrEmpty(oldPagination.Cursor))
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

            // 最大 20 回ループ。 許容範囲
            var (httpResponse, msg) = await GetHttpResponseWithRetryAsync(url, token, "get streams");
            if (httpResponse is null)
            {
                ShowLog(msg);
                continue;
            }

            using (httpResponse)
            {
                var json = await httpResponse.Content.ReadAsStringAsync();

                // TwitchClipSearchResponse 型に変換
                var searchResponse = JsonSerializer.Deserialize<TwitchStreamSearchResponse>(json);
                if (searchResponse is null)
                {
                    return new();
                }

                pagination = searchResponse.Pagination;

                // vtuber かどうかのフィルタリング
                var vtuberStream = FilterVtuberStream(searchResponse);

                // さらに企業勢をはじく
                var filter = await _cache.GetVtuberFilterListAsync(CacheKeyHelper.GetVtuberCachKey(VIdeoPlatform.Twitch));
                var filteredStream = vtuberStream
                    .Where(v => !filter.Contains(v.UserId));

                data.AddRange(filteredStream);

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
        }

        return new TwitchStreamSearchResponse
        {
            Data = data,
            Pagination = pagination
        };
    }

    /// <summary>
    /// Tag に "Vtuber" が存在する配信のみを取得
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    private List<TwitchStreamSearchRaw> FilterVtuberStream(TwitchStreamSearchResponse response)
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

            var (httpResponse, msg) = await GetHttpResponseWithRetryAsync(url, token, "get users");
            if (httpResponse is null)
            {
                ShowLog(msg);
                continue;
            }

            using (httpResponse)
            {

                var json = await httpResponse.Content.ReadAsStringAsync();
                var userResponse = JsonSerializer.Deserialize<TwitchUserResponse>(json);

                if (userResponse is not null)
                {
                    allUsers.AddRange(userResponse.Data);
                }
            }
        }

        return new TwitchUserResponse { Data = allUsers };
    }

    /// <summary>
    /// リトライを含めリクエストを送る
    /// 目的は 503 対策
    /// </summary>
    /// <param name="request"></param>
    /// <param name="requestMethodName"></param>
    /// <returns></returns>
    private async Task<(HttpResponseMessage? response, string errorMsg)> GetHttpResponseWithRetryAsync(
        string url,
        string token,
        string requestMethodName)
    {
        var maxRetry = 3;

        for (var tryCount = 0; tryCount <= maxRetry; tryCount++)
        {
            using var request = CreateHttpRequestWithHeader(url, token);
            var response = await _httpClient.SendAsync(request);

            // 成功した場合は即 return
            if (response.IsSuccessStatusCode)
            {
                return (response, "");
            }

            // 失敗の場合は response を dispose
            using (response)
            {
                // 503 の場合は数秒待機して continue
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
                // その他のエラーは即 return
                else if (!response.IsSuccessStatusCode)
                {
                    return (null, $"{requestMethodName} {response.StatusCode}エラー。");
                }
            }
        }

        return (null, $"{requestMethodName} 503エラー。");
    }

    private HttpRequestMessage CreateHttpRequestWithHeader(string url, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Client-ID", _settings.ClientId);
        request.Headers.Add("Authorization", $"Bearer {token}");
        return request;
    }

    private VideoDataResponse ToDTO(List<TwitchStreamSearchRaw> sData, List<TwitchUserRaw> cData)
    {
        if (sData.Count == 0 || cData.Count == 0)
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

    private VideoDataResponse CreateDataForDebug()
    {
        var dtos = new List<VideoDataDTO>();
        for (var i = 0; i < 10; i++)
        {
            var d = new VideoDataDTO
            {
                VideoId = "",
                VideoTitle = "テスト",
                ChannelId = "akamikarubi",
                ChannelName = "テスト",
                SearchHighTumbnail = new VideoHighThumbnailDTO
                {
                    Url = $"https://static-cdn.jtvnw.net/previews-ttv/live_user_ukyochi_jp-1280x720.jpg",
                    Width = 1280,
                    Height = 720,
                },
                ChannelHighThumbnail = new VideoHighThumbnailDTO
                {
                    Url = $"https://static-cdn.jtvnw.net/jtv_user_pictures/f5ba0ca0-2187-41ea-b7bb-d0457b1dba0e-profile_image-70x70.png",
                    Width = 300,
                    Height = 300,
                },
                Platform = VIdeoPlatform.Twitch
            };

            dtos.Add(d);
        }

        return new()
        {
            Items = dtos,
        };
    }
}