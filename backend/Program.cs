using MyApi.Config;
using MyApi.Endpoints;
using MyApi.Interfaces;
using MyApi.Services;
using MyApi.Services.CacheUpdater;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// CORS を追加
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

// 初回起動時のキャッシュ更新処理を追加
builder.Services.AddHostedService<StartupCacheUpdater>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    // db を操作する api のパスであった場合、自身の WPF アプリから以外の場合ははじく
    if (context.Request.Path.StartsWithSegments("/api/vtuberData"))
    {
        // WPF アプリ側でヘッダーを付加済み
        var apiKey = context.Request.Headers["X-DB-Api-Key"].ToString();
        var expectedKey = builder.Configuration["DBAdminApiKey"];

        if (apiKey != expectedKey)
        {
            context.Response.StatusCode = 401;
            return;
        }

        await next();
        return;
    }

    // API へのアクセス、かつ GET リクエストの時だけカウント
    if (!context.Request.Path.StartsWithSegments("/api/videos") || !(context.Request.Method == "GET"))
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
        Secure = false,   // nginx が https 通信を行い、docker ネットワーク内では http なので false
        SameSite = SameSiteMode.Strict // 他のサイトからのリクエストには同梱しない
    });

    await next();
});

// CORS を有効化
app.UseCors();

// エンドポイント登録
app.MapVideoEndpoints();
app.MapVtuberEndpoints();

app.Run();