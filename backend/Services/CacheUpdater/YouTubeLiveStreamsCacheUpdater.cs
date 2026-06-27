using MyApi.DTOs;
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
        var cache = scope.ServiceProvider.GetRequiredService<IRedisCacheService>();

        var startTime = DateTime.Now;
        _logger.LogInformation("\n{StartTime} YouTube キャッシュ更新開始\n", startTime);

        VideoDataResponse? response = null;
        try
        {
            response = await youTubeService.FetchYouTubeLiveStreamsAsync();
        }
        catch (OperationCanceledException)
        {
            var failTime = DateTime.Now;
            _logger.LogInformation("\n{FailTime} YouTube キャッシュ更新タイムアウト\n", failTime);
            return;
        }
        catch (Exception ex)
        {
            var failTime = DateTime.Now;
            var message = ex.Message;
            _logger.LogInformation("\n{FailTime} YouTube キャッシュ更新エラー {Message} \n", failTime, message);
            return;
        }

        // 結果が 0 件の場合は return
        if (response.Items.Count == 0)
        {
            var noItemsFinishTime = DateTime.Now;
            _logger.LogInformation("\n{FinishTime} YouTube response  0 件のためキャッシュ更新せず\n", noItemsFinishTime);
            return;
        }

        var json = JsonSerializer.Serialize(response);

        var cacheKey = CacheKeyHelper.GetCacheKey(VideoType.YouTubeLiveStream);
        await cache.SetStringAsync(
            cacheKey,
            json,
            TimeSpan.FromDays(1) // 最大で 1 日は保存しておく
        );

        var finishTime = DateTime.Now;
        _logger.LogInformation("\n{FinishTime} YouTube キャッシュ更新完了\n", finishTime);
    }

    //TODO: クォータ増加したら更新頻度増やす
    //クォータが増加したので 30 分 → 5 分に変更
    // youtube は 5 分毎に更新
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
        ).AddMinutes(5);

        return next - now;
    }
}