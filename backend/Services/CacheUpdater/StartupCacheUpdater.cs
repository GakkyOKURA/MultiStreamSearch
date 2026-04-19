using MyApi.Interfaces;
using MyApi.Models;
using System.Text.Json;

namespace MyApi.Services.CacheUpdater;

public class StartupCacheUpdater : IHostedService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<StartupCacheUpdater> _logger;

    public StartupCacheUpdater(IServiceProvider provider, ILogger<StartupCacheUpdater> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _provider.CreateScope();

        var cache = scope.ServiceProvider.GetRequiredService<IRedisCacheService>();

        await StartupDB(scope, cache);

        await StartupYouTube(scope, cache);

        await StartupTwitch(scope, cache);

        await StartupAI(scope, cache);
    }

    private async Task StartupDB(IServiceScope scope, IRedisCacheService cache)
    {
        // まずは DB の初期化
        var repo = scope.ServiceProvider.GetRequiredService<IVtuberRepository>();
        await repo.InitializeDatabaseAsync();

        // Filter をキャッシュに保存
        _logger.LogInformation("\nVtuberData starts updating cache before app run\n");

        // YouTube の Filter をキャッシュに保存
        var vtuberYoutubeData = await repo.GetVtubersByFilterAsync("", "", VIdeoPlatform.YouTube.ToString());
        await cache.SetVtuberFilterListAsync(
            CacheKeyHelper.GetVtuberCachKey(VIdeoPlatform.YouTube),
            vtuberYoutubeData.Items.
            Select(v => v.ChannelId));

        // つぎは Twitch
        var vtuberTwitchData = await repo.GetVtubersByFilterAsync("", "", VIdeoPlatform.Twitch.ToString());
        await cache.SetVtuberFilterListAsync(
            CacheKeyHelper.GetVtuberCachKey(VIdeoPlatform.Twitch),
            vtuberTwitchData.Items.
            Select(v => v.ChannelId));

        _logger.LogInformation("\nVtuberData finished updating cache before app run\n");
    }

    private async Task StartupYouTube(IServiceScope scope, IRedisCacheService cache)
    {
        // 配信データをキャッシュに保存
        _logger.LogInformation("\nYouTube starts updating cache before app run\n");

        // YouTube のデータを取得してキャッシュに保存
        var youTubeService = scope.ServiceProvider.GetRequiredService<IYouTubeService>();
        var youTubeResponse = await youTubeService.FetchYouTubeLiveStreamsAsync();
        var jsonYouTube = JsonSerializer.Serialize(youTubeResponse);
        await cache.SetStringAsync(
            CacheKeyHelper.GetCacheKey(VideoType.YouTubeLiveStream), 
            jsonYouTube,
            TimeSpan.FromMinutes(65));

        _logger.LogInformation("\nYouTube finished updating cache before app run\n");
    }

    private async Task StartupTwitch(IServiceScope scope, IRedisCacheService cache)
    {
        _logger.LogInformation("\nTwitch starts updating cache before app run\n");

        // Twitch のデータを取得してキャッシュに保存
        var twitchService = scope.ServiceProvider.GetRequiredService<ITwitchService>();
        var twitchResponse = await twitchService.FetchTwitchStreamsAsync();
        var jsonTwitch = JsonSerializer.Serialize(twitchResponse);
        await cache.SetStringAsync(
            CacheKeyHelper.GetCacheKey(VideoType.TwitchStream),
            jsonTwitch, 
            TimeSpan.FromMinutes(10));

        _logger.LogInformation("\nTwitch finished updating cache before app run\n");
    }

    private async Task StartupAI(IServiceScope scope, IRedisCacheService cache)
    {
        // AI 要約
        _logger.LogInformation("\nAiSummary starts updating cache before app run\n");

        var aiService = scope.ServiceProvider.GetRequiredService<IAiService>();
        var aiResponse = await aiService.FetchVtuberAnalysis();
        var jsonAi = JsonSerializer.Serialize(aiResponse);
        await cache.SetStringAsync(
            CacheKeyHelper.GetCacheKey(VideoType.AiSummary), 
            jsonAi, 
            TimeSpan.FromMinutes(65));

        _logger.LogInformation("\nAiSummary finished updating cache before app run\n");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

