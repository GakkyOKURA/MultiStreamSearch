using MyApi.Config;
using MyApi.Endpoints;
using MyApi.Interfaces;
using MyApi.Services;

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

builder.Services.AddHttpClient<IYouTubeService, YouTubeService>();
builder.Services.AddHttpClient<ITwitchService, TwitchService>();

var app = builder.Build();

// ★ CORS を有効化（UseHttpsRedirection の前）
app.UseCors();
app.UseHttpsRedirection();

// エンドポイント登録
app.MapYouTubeEndpoints(); 
app.MapTwitchEndpoints();

app.Run();
