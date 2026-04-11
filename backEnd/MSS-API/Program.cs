using MyApi.Config;
using MyApi.Endpoints;
using MyApi.Interfaces;
using MyApi.Models;
using MyApi.Services;
using MyApi.Services.CacheUpdater;
using StackExchange.Redis;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ★ https 用に CORS を追加
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 設定値読み込み
builder.Services.Configure<YouTubeApiSettings>(
    builder.Configuration.GetSection("YouTubeApi"));
builder.Services.Configure<TwitchApiSettings>(
    builder.Configuration.GetSection("TwitchApi"));
builder.Services.Configure<AiApiSettings>(
    builder.Configuration.GetSection("AiApi"));

// Redis を追加
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = builder.Configuration.GetConnectionString("Redis");
    return ConnectionMultiplexer.Connect(config!);
});

builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();

// Postgres を追加
builder.Services.AddSingleton<IVtuberRepository>(sp =>
{
    var config = builder.Configuration.GetConnectionString("PostgreSQL");
    return new VtuberRepository(config!);
});


builder.Services.AddHttpClient<IYouTubeService, YouTubeService>();
builder.Services.AddHttpClient<ITwitchService, TwitchService>();
builder.Services.AddHttpClient<IAiService, AiSummaryService>();
builder.Services.AddScoped<IVideoService, VideoService>();

// YouTube のキャッシュ更新サービスを追加
builder.Services.AddHostedService<YouTubeLiveStreamsCacheUpdater>();
// Twitch のキャッシュ更新サービスを追加
builder.Services.AddHostedService<TwitchStreamsCacheUpdater>();
// Gemini のキャッシュ更新サービスを追加
builder.Services.AddHostedService<AiSummaryCacheUpdater>();
// vtuberData のキャッシュ更新サービスを追加
builder.Services.AddHostedService<VtuberDataCacheUpdater>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var cache = scope.ServiceProvider.GetRequiredService<IRedisCacheService>();
    // 1. まずは DB の初期化
    var repo = scope.ServiceProvider.GetRequiredService<IVtuberRepository>();
    await repo.InitializeDatabaseAsync();

    // 2. Filter をキャッシュに保存
    var vtuberDbLogger = scope.ServiceProvider.GetRequiredService<ILogger<VtuberDataCacheUpdater>>();
    vtuberDbLogger.LogInformation("\nVtuberData キャッシュ更新開始\n");

    // 2-1. YouTube の Filter をキャッシュに保存
    var vtuberYoutubeData = await repo.GetVtubersByFilterAsync("", "", VIdeoPlatform.YouTube.ToString());
    await cache.SetVtuberFilterListAsync(
        CacheKeyHelper.GetVtuberCachKey(VIdeoPlatform.YouTube),
        vtuberYoutubeData.Items.
        Select(v => v.ChannelId));
    // 2-2. つぎは Twitch
    var vtuberTwitchData = await repo.GetVtubersByFilterAsync("", "", VIdeoPlatform.Twitch.ToString());
    await cache.SetVtuberFilterListAsync(
        CacheKeyHelper.GetVtuberCachKey(VIdeoPlatform.Twitch),
        vtuberTwitchData.Items.
        Select(v => v.ChannelId));

    vtuberDbLogger.LogInformation("\nVtuberData キャッシュ更新完了\n");

    // 3. 配信データをキャッシュに保存
    var youTubeLogger = scope.ServiceProvider.GetRequiredService<ILogger<YouTubeLiveStreamsCacheUpdater>>();
    youTubeLogger.LogInformation("\nYouTube キャッシュ更新開始\n");

    // 3-1. YouTube のデータを取得してキャッシュに保存
    var youTubeService = scope.ServiceProvider.GetRequiredService<IYouTubeService>();
    var youTubeResponse = await youTubeService.FetchYouTubeLiveStreamsAsync();
    var jsonYouTube = JsonSerializer.Serialize(youTubeResponse);
    await cache.SetStringAsync(CacheKeyHelper.GetCacheKey(VideoType.YouTubeLiveStream), jsonYouTube, TimeSpan.FromMinutes(65));

    youTubeLogger.LogInformation("\nYouTube キャッシュ更新完了\n");

    var twitchLogger = scope.ServiceProvider.GetRequiredService<ILogger<TwitchStreamsCacheUpdater>>();
    twitchLogger.LogInformation("\nTwitch キャッシュ更新開始\n");

    // 3-2. つぎは Twitch
    var twitchService = scope.ServiceProvider.GetRequiredService<ITwitchService>();
    var twitchResponse = await twitchService.FetchTwitchStreamsAsync();
    var jsonTwitch = JsonSerializer.Serialize(twitchResponse);
    await cache.SetStringAsync(CacheKeyHelper.GetCacheKey(VideoType.TwitchStream), jsonTwitch, TimeSpan.FromMinutes(10));

    twitchLogger.LogInformation("\nTwitch キャッシュ更新完了\n");

    // 4. 最後に AI 要約
    var aiLogger = scope.ServiceProvider.GetRequiredService<ILogger<AiSummaryCacheUpdater>>();
    aiLogger.LogInformation("\nAiSummary キャッシュ更新開始\n");

    var aiService = scope.ServiceProvider.GetRequiredService<IAiService>();
    var aiResponse = await aiService.FetchVtuberAnalysis();
    var jsonAi = JsonSerializer.Serialize(aiResponse);
    await cache.SetStringAsync(CacheKeyHelper.GetCacheKey(VideoType.AiSummary), jsonAi, TimeSpan.FromMinutes(65));

    aiLogger.LogInformation("\nAiSummary キャッシュ更新完了\n");
}

app.Use(async (context, next) =>
{
    // API へのアクセス、かつ GET リクエストの時だけカウント
    if (!context.Request.Path.StartsWithSegments("/api") || !(context.Request.Method == "GET"))
    {
        await next();
        return;
    }

    // User-Agent が自分の WPF アプリだったらカウントしない
    var userAgent = context.Request.Headers["User-Agent"].ToString();
    if (userAgent.Contains("VtuberDbMgr"))
    {
        await next();
        return;
    }

    // キー名を設定
    const string cookieName = "is_visited";

    // クッキーがあるかチェック
    if (context.Request.Cookies.ContainsKey(cookieName))
    {
        await next();
        return;
    }

    // なければカウントアップ
    var counter = context.RequestServices.GetRequiredService<IVtuberRepository>();
    await counter.IncrementAsync();

    // クッキーを焼く（24時間有効）
    // キー名：is_visited 値： true(ここは何でもいい)
    context.Response.Cookies.Append(cookieName, "true", new CookieOptions
    {
        Expires = DateTimeOffset.Now.AddDays(1),
        HttpOnly = true, // JavaScript から触らせない。クロスサイトスクリプティング攻撃対策
        Secure = false,   // HTTPS の時は true だが、今回は http なので false
        SameSite = SameSiteMode.Strict // 他のサイトからのリクエストには同梱しない
    });

    await next();
});


// CORS を有効化
app.UseCors();
//app.UseHttpsRedirection();

// エンドポイント登録
app.MapVideoEndpoints();
app.MapVtuberEndpoints();


app.Run();