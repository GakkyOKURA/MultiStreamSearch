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
        // この中を無限ループ
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetDelayUntilNextUpdate();
            await Task.Delay(delay, stoppingToken);
            // youtube と更新時間がかぶっているので、さらに少し待って youtube を先に終わらせる。
            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

            await UpdateCache(stoppingToken);
        }
    }

    private async Task UpdateCache(CancellationToken token)
    {
        using var scope = _provider.CreateScope();

        var geminiService = scope.ServiceProvider.GetRequiredService<IAiService>();
        var cache = scope.ServiceProvider.GetRequiredService<IRedisCacheService>();

        var startTime = DateTime.Now;
        _logger.LogInformation("\n{StartTime} AiSummary キャッシュ更新開始\n", startTime);

        ChannelSummaryResponse? response = null;
        try
        {
            response = await geminiService.FetchVtuberAnalysis();
        }
        catch (OperationCanceledException)
        {
            var failTime = DateTime.Now;
            _logger.LogInformation("\n{FailTime} AiSummary キャッシュ更新タイムアウト\n", failTime);
            return;
        }
        catch (Exception ex)
        {
            var failTime = DateTime.Now;
            var message = ex.Message;
            _logger.LogInformation("\n{FailTime} AiSummary キャッシュ更新エラー {Message} \n", failTime, message);
            return;
        }

        if (response.Analyses.Count == 0)
        {
            var noItemsFinishTime = DateTime.Now;
            _logger.LogInformation("\n{FinishTime} Aisummary response 0 件のためキャッシュ更新せず\n", noItemsFinishTime);
            return;
        }

        var json = JsonSerializer.Serialize(response);

        var cacheKey = CacheKeyHelper.GetCacheKey(VideoType.AiSummary);
        await cache.SetStringAsync(
            cacheKey,
            json,
            TimeSpan.FromDays(1) // 最大で 1 日は保存
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