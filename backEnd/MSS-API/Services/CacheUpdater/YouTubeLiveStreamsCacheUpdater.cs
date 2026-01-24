using MyApi.Interfaces;
using MyApi.Models;
using System.Text.Json;

namespace MyApi.Services.CacheUpdater;

public class YouTubeLiveStreamsCacheUpdater : BackgroundService
{
    private readonly ILogger<YouTubeLiveStreamsCacheUpdater> _logger;
    private readonly IServiceProvider _provider;

    public YouTubeLiveStreamsCacheUpdater(
        ILogger<YouTubeLiveStreamsCacheUpdater> logger,
        IServiceProvider provider)
    {
        _logger = logger;
        _provider = provider;
    }

    // Program.cs で AddHostedService を登録した瞬間に “常駐サービス” となり、
    // アプリ起動時に自動で実行される
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 起動時にまず更新
        await UpdateCache(stoppingToken);

        // この中を無限ループ
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetDelayUntilNextUpdate();
            await Task.Delay(delay, stoppingToken);

            await UpdateCache(stoppingToken);
        }
    }

    // 同じキーに対して新しい値を Set すると、古い値は自動的に上書きされて消える。
    // TTL（有効期限）も新しく設定される。だからTTLは長めでも大丈夫。
    private async Task UpdateCache(CancellationToken token)
    {
        using var scope = _provider.CreateScope();

        var youTubeService = scope.ServiceProvider.GetRequiredService<IYouTubeService>();
        var cache = scope.ServiceProvider.GetRequiredService<RedisCacheService>();

        _logger.LogInformation("YouTube キャッシュ更新開始…");

        var keywords = SearchWordHelper.GameIds.Keys;
        foreach (var keyword in keywords)
        {
            // バックエンド専用 API 呼び出し
            var ytsr = await youTubeService.FetchYouTubeLiveStreamsAsync(keyword);
            var json = JsonSerializer.Serialize(ytsr);

            var cacheKey = CacheKeyHelper.GetCacheKey(CacheKeyHelper.VideoType.YouTubeLiveStream, keyword);
            await cache.SetStringAsync(
                cacheKey,
                json,
                TimeSpan.FromHours(3) // TTLは長め
            );
        }

        _logger.LogInformation("YouTube キャッシュ更新完了");
    }


    private TimeSpan GetDelayUntilNextUpdate()
    {
        var now = DateTime.Now;

        // コアタイム：18:00〜翌2:00
        bool isCoreTime = now.Hour >= 18 || now.Hour < 2;

        // 次の更新時刻（00分ジャスト）
        int nextHour = isCoreTime
            ? now.Hour + 1   // コアタイムは1時間後
            : now.Hour + 2;  // 非コアタイムは2時間後

        // 日付をまたぐ場合に備えて DateTime を正しく構築
        var next = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0)
            .AddHours(nextHour);

        return next - now;
    }

}

