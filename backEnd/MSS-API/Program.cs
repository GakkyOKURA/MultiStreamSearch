using MyApi.Config;
using MyApi.Endpoints;
using MyApi.Interfaces;
using MyApi.Models;
using MyApi.Services;
using MyApi.Services.CacheUpdater;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// ★ https 用に CORS を追加（builder.Build() の前）
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ★ appsettings.json のセクションを ApiSettings にバインド
//ASP.NET Core は起動時に appsettings.json を読み込み、
//builder.Services.Configure<ApiSettings>(...) を書いた瞬間に
//設定値が IOptions にバインドされる。
//Sectionを指定すると、キーと ApiSettings のプロパティが一致している
//場合は自動で値をセットしてくれる
builder.Services.Configure<YouTubeApiSettings>(
    builder.Configuration.GetSection("YouTubeApi"));
builder.Services.Configure<TwitchApiSettings>(
    builder.Configuration.GetSection("TwitchApi"));

// ★ Redis を追加
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = builder.Configuration.GetConnectionString("Redis");
    return ConnectionMultiplexer.Connect(config!);
});

builder.Services.AddSingleton<RedisCacheService>();


builder.Services.AddHttpClient<IYouTubeService, YouTubeService>();
builder.Services.AddHttpClient<ITwitchService, TwitchService>();
builder.Services.AddScoped<IGameInfoProvider, GameInfoProvider>();

// ★ YouTube のキャッシュ更新サービスを追加
builder.Services.AddHostedService<YouTubeLiveStreamsCacheUpdater>();
builder.Services.AddHostedService<YouTubeShortsCacheUpdater>();
// ★ Twitch のキャッシュ更新サービスを追加
builder.Services.AddHostedService<TwitchStreamsCacheUpdater>();
builder.Services.AddHostedService<TwitchClipsCacheUpdater>();



var app = builder.Build();

// ★ CORS を有効化（UseHttpsRedirection の前）
app.UseCors();
app.UseHttpsRedirection();

// エンドポイント登録
app.MapYouTubeEndpoints(); 
app.MapTwitchEndpoints();
app.MapGameInfoEndpoints();

app.Run();
