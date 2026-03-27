using MyApi.Interfaces;
using MyApi.Models;
using System.Text.Json;

namespace MyApi.Services.CacheUpdater;

public class AiSummaryCacheUpdater : BackgroundService
{
    private readonly ILogger<AiSummaryCacheUpdater> _logger;
    private readonly IServiceProvider _provider;

    public AiSummaryCacheUpdater(
        ILogger<AiSummaryCacheUpdater> logger,
        IServiceProvider provider)
    {
        _logger = logger;
        _provider = provider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // youtube と twitch のデータ収集が終わってからでないと意味がないので
        //最初は 2 分待機
        await Task.Delay(TimeSpan.FromMinutes(2));
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

        var geminiService = scope.ServiceProvider.GetRequiredService<IAiService>();
        var cache = scope.ServiceProvider.GetRequiredService<RedisCacheService>();

        var startTime = DateTime.Now;
        _logger.LogInformation("\n{StartTime} AiSummary キャッシュ更新開始...\n", startTime);

        ChannelSummaryResponse? response = null;
        try
        {
            response = await geminiService.FetchVtuberAnalysis();
        }
        catch(OperationCanceledException)
        {
            var failTime = DateTime.Now;
            _logger.LogInformation("\n{FailTime} AiSummary キャッシュタイムアウト\n", failTime);
            return;
        }

        var json = JsonSerializer.Serialize(response);

        var cacheKey = CacheKeyHelper.GetCacheKey(CacheKeyHelper.VideoType.AiSummary);
        await cache.SetStringAsync(
            cacheKey,
            json,
            TimeSpan.FromMinutes(65)
        );

        var finishTime = DateTime.Now;
        _logger.LogInformation("\n{FinishTime} AiSummary キャッシュ更新完了\n", finishTime);
    }

    // ai は 30分ごとに更新
    private TimeSpan GetDelayUntilNextUpdate()
    {
        var now = DateTime.Now;

        // 次の分の00秒
        var next = new DateTime(
            now.Year,
            now.Month,
            now.Day,
            now.Hour,
            now.Minute,
            0
        ).AddMinutes(30);

        return next - now;
    }
}
