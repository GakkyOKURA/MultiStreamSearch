using MyApi.Config;
using MyApi.Endpoints;
using MyApi.Interfaces;
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
builder.Services.Configure<AiApiSettings>(
    builder.Configuration.GetSection("AiApi"));

// Redis を追加
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = builder.Configuration.GetConnectionString("Redis");
    return ConnectionMultiplexer.Connect(config!);
});

builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();

builder.Services.AddSingleton(sp =>
{
    var config = builder.Configuration.GetConnectionString("PostgreSQL");
    return new VtuberRepository(config!);
});


//builder.Services.AddHttpClient<IYouTubeService, YouTubeService>();
//builder.Services.AddHttpClient<ITwitchService, TwitchService>();
//builder.Services.AddHttpClient<IAiService, AiSummaryService>();
//builder.Services.AddScoped<IVideoService, VideoService>();

//// YouTube のキャッシュ更新サービスを追加
//builder.Services.AddHostedService<YouTubeLiveStreamsCacheUpdater>();
//// Twitch のキャッシュ更新サービスを追加
//builder.Services.AddHostedService<TwitchStreamsCacheUpdater>();
//// Gemini のキャッシュ更新サービスを追加
//builder.Services.AddHostedService<AiSummaryCacheUpdater>();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var repo = scope.ServiceProvider.GetRequiredService<VtuberRepository>();
await repo.InitializeDatabaseAsync();

// CORS を有効化（UseHttpsRedirection の前）
app.UseCors();
app.UseHttpsRedirection();

// エンドポイント登録
//app.MapVideoEndpoints();
app.MapVtuberEndpoints();

app.Run();