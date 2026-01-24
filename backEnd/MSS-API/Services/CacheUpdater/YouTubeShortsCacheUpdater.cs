using MyApi.Interfaces;
using MyApi.Models;
using System.Text.Json;

namespace MyApi.Services.CacheUpdater;

public class YouTubeShortsCacheUpdater : BackgroundService
{
    private readonly ILogger<YouTubeShortsCacheUpdater> _logger;
    private readonly IServiceProvider _provider;

    public YouTubeShortsCacheUpdater(
        ILogger<YouTubeShortsCacheUpdater> logger,
        IServiceProvider provider)
    {
        _logger = logger;
        _provider = provider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 起動時にまず更新
        await UpdateShortsCache(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetDelayUntilMidnight();
            await Task.Delay(delay, stoppingToken);

            await UpdateShortsCache(stoppingToken);
        }
    }

    private async Task UpdateShortsCache(CancellationToken token)
    {
        using var scope = _provider.CreateScope();

        var youTubeService = scope.ServiceProvider.GetRequiredService<IYouTubeService>();
        var cache = scope.ServiceProvider.GetRequiredService<RedisCacheService>();

        _logger.LogInformation("YouTube Shorts キャッシュ更新開始…");

        var keywords = SearchWordHelper.GameIds.Keys;

        foreach (var keyword in keywords)
        {
            var ytsr = await youTubeService.FetchYouTubeShortsAsync(keyword);
            var json = JsonSerializer.Serialize(ytsr);

            var cacheKey = CacheKeyHelper.GetCacheKey(CacheKeyHelper.VideoType.YouTubeShort, keyword);
            await cache.SetStringAsync(
                cacheKey,
                json,
                TimeSpan.FromHours(30) // TTL長め
            );
        }

        _logger.LogInformation("YouTube Shorts キャッシュ更新完了");
    }

    // Shorts は毎日0時に更新
    private TimeSpan GetDelayUntilMidnight()
    {
        var now = DateTime.Now;
        var tomorrow = now.Date.AddDays(1); // 翌日の 00:00
        return tomorrow - now;
    }
}

