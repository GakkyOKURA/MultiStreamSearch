using MyApi.Interfaces;
using MyApi.Models;
using System.Text.Json;

namespace MyApi.Services.CacheUpdater;

public class TwitchClipsCacheUpdater : BackgroundService
{
    private readonly ILogger<TwitchClipsCacheUpdater> _logger;
    private readonly IServiceProvider _provider;

    public TwitchClipsCacheUpdater(
        ILogger<TwitchClipsCacheUpdater> logger,
        IServiceProvider provider)
    {
        _logger = logger;
        _provider = provider;
    }

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

    private async Task UpdateCache(CancellationToken token)
    {
        using var scope = _provider.CreateScope();

        var twitchService = scope.ServiceProvider.GetRequiredService<ITwitchService>();
        var cache = scope.ServiceProvider.GetRequiredService<RedisCacheService>();

        var startTime = DateTime.Now;
        // CA2254 対策
        _logger.LogInformation("{StartTime} Twitch Clip キャッシュ更新開始...", startTime);

        var gameIds = SearchWordHelper.GameIds.Values;
        foreach (var gameId in gameIds)
        {
            var result = await twitchService.FetchTwitchClipsAsync(gameId);
            var json = JsonSerializer.Serialize(result);

            var cacheKey = CacheKeyHelper.GetCacheKey(CacheKeyHelper.VideoType.TwitchClip, gameId);
            await cache.SetStringAsync(
                cacheKey,
                json,
                TimeSpan.FromHours(2)
            );
        }

        var finishTime = DateTime.Now;
        _logger.LogInformation("{FinishTime} Twitch Clip キャッシュ更新完了", finishTime);
    }

    // Clip は1時間ごとに更新
    private TimeSpan GetDelayUntilNextUpdate()
    {
        var now = DateTime.Now;
        var next = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0)
            .AddHours(now.Hour + 1);

        return next - now;
    }
}
