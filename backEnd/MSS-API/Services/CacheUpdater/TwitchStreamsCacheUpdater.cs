using MyApi.Interfaces;
using MyApi.Models;
using System.Text.Json;

namespace MyApi.Services.CacheUpdater;

public class TwitchStreamsCacheUpdater : BackgroundService
{
    private readonly ILogger<TwitchStreamsCacheUpdater> _logger;
    private readonly IServiceProvider _provider;

    public TwitchStreamsCacheUpdater(
        ILogger<TwitchStreamsCacheUpdater> logger,
        IServiceProvider provider )
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
        _logger.LogInformation("{StartTime} Twitch Stream キャッシュ更新開始...", startTime);

        var gameIds = SearchWordHelper.GameIds.Values;
        foreach ( var gameId in gameIds )
        {
            var result = await twitchService.FetchTwitchStreamsAsync(gameId);
            var json = JsonSerializer.Serialize(result);

            var cacheKey = CacheKeyHelper.GetCacheKey(CacheKeyHelper.VideoType.TwitchStream, gameId);
            await cache.SetStringAsync(
                cacheKey,
                json,
                TimeSpan.FromMinutes(2)
            );
        }

        var finishTime = DateTime.Now;
        _logger.LogInformation("{FinishTime} Twitch Stream キャッシュ更新完了", finishTime);
    }

    // Twitch は API 制限が緩いので1分ごとに更新
    private TimeSpan GetDelayUntilNextUpdate()
    {
        var now = DateTime.Now;

        //TODO:Twitch API は1分毎にアクセス上限を設けているので、
        //Streams の検索と Clips の検索は同じ分に行わないほうがいい

        // 次の分の00秒
        var next = new DateTime(
            now.Year,
            now.Month,
            now.Day,
            now.Hour,
            now.Minute,
            0
        ).AddMinutes(1);

        return next - now;
    }

}
