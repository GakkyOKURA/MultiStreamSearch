using MyApi.Interfaces;
using MyApi.Models;

namespace MyApi.Services.CacheUpdater;

public class VtuberDataCacheUpdater : BackgroundService
{
    private readonly ILogger<VtuberDataCacheUpdater> _logger;
    private readonly IServiceProvider _provider;

    public VtuberDataCacheUpdater(
        ILogger<VtuberDataCacheUpdater> logger,
        IServiceProvider provider)
    {
        _logger = logger;
        _provider = provider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetDelayUntilNextUpdate();
            await Task.Delay(delay, stoppingToken);

            await UpdateCache(stoppingToken);
        }
    }

    internal async Task UpdateCache(CancellationToken token)
    {
        using var scope = _provider.CreateScope();

        var vtuberData = scope.ServiceProvider.GetRequiredService<IVtuberRepository>();
        var cache = scope.ServiceProvider.GetRequiredService<IRedisCacheService>();

        var startTime = DateTime.Now;
        _logger.LogInformation("\n{StartTime} VtuberData キャッシュ更新開始\n", startTime);

        // YouTube
        VtuberResponse? responseYouTube = null;
        try
        {
            responseYouTube = await vtuberData.GetVtubersByFilterAsync(
                "", "", CacheKeyHelper.GetVtuberCachKey(VIdeoPlatform.YouTube));
        }
        catch
        {
            var failTime = DateTime.Now;
            _logger.LogInformation("\n{FailTime} VtuberData キャッシュ失敗\n", failTime);
        }

        await cache.SetVtuberFilterListAsync(
            $"VtuberData{VIdeoPlatform.YouTube}",
            responseYouTube!.Items
            .Select(v => v.ChannelId));

        // Twitch
        VtuberResponse? responseTwitch = null;
        try
        {
            responseTwitch = await vtuberData.GetVtubersByFilterAsync(
                "", "", CacheKeyHelper.GetVtuberCachKey(VIdeoPlatform.Twitch));
        }
        catch
        {
            var failTime = DateTime.Now;
            _logger.LogInformation("\n{FailTime} VtuberData キャッシュ失敗\n", failTime);
        }

        await cache.SetVtuberFilterListAsync(
            $"VtuberData{VIdeoPlatform.Twitch}",
            responseTwitch!.Items
            .Select(v => v.ChannelId));

        var finishTime = DateTime.Now;
        _logger.LogInformation("\n{FinishTime} VtuberData キャッシュ更新完了\n", finishTime);
    }

    private TimeSpan GetDelayUntilNextUpdate()
    {
        var now = DateTime.Now;

        // 次の日
        var next = new DateTime(
            now.Year,
            now.Month,
            now.Day,
            0,
            0,
            0
        ).AddDays(1);

        return next - now;
    }
}