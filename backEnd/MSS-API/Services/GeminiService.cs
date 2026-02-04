using Microsoft.Extensions.Options;
using MyApi.Config;
using MyApi.Interfaces;
using MyApi.Models;
using System.Text.Json;
using MyApi.CutomException;

namespace MyApi.Services;

public class GeminiService : IGeminiService
{
    private readonly RedisCacheService _cache;
    private readonly HttpClient _httpClient;
    private readonly GeminiApiSettings _settings;

    private const string ModelId = "gemini-2.5-flash-lite";

    public GeminiService(
        RedisCacheService cache,
        HttpClient httpClient, // SDK の Client から HttpClient に変更
        IOptions<GeminiApiSettings> settings)
    {
        _cache = cache;
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<ChannelAnalysisResponse> SearchVtuberAnalysis()
    {
        var cacheKey = CacheKeyHelper.GetCacheKey(CacheKeyHelper.VideoType.GeminiAnakysis);
        return await _cache.GetAsync<ChannelAnalysisResponse>(cacheKey) ?? new();
    }

    // BackgroundService から呼ばれる：最新データを取得して Gemini で解析する
    public async Task<ChannelAnalysisResponse> FetchVtuberAnalysis()
    {
        var vData = await GetVtuberData();
        return await GenerateAnalysisAsync(vData);
    }

    private async Task<List<ProvideingVtuberData>> GetVtuberData()
    {
        var combinedList = await GetCombinedList();

        return combinedList
            .OrderBy(_ => Guid.NewGuid()) // ランダムに並び替えて...
            .Take(10) // 10 個取得
            .Select(v => new ProvideingVtuberData
            {
                ChannelId = v.ChannelId,
                VideoDescription = v.SearchDescription,
                ChannelDescription = v.ChannelDescription,
                Keywords = v.Keywords,
                Tags = v.TopicCategories
            })
            .ToList();
    }

    private async Task<List<VideoDataDTO>> GetCombinedList()
    {
        var youtubeData = await GetYouTubeLiveStreamsAsync();
        var twitchData = await GetTwitchStreamsAsync();

        return youtubeData.Items.Concat(twitchData.Items).ToList();
    }

    // YouTubeService.SearchYouTubeLiveStreamsAsync との DRY 原則には違反していない
    private async Task<VideoDataResponse> GetYouTubeLiveStreamsAsync()
    {
        var cacheKey = CacheKeyHelper.GetCacheKey(CacheKeyHelper.VideoType.YouTubeLiveStream);
        return await _cache.GetAsync<VideoDataResponse>(cacheKey) ?? new();
    }

    // TwitchService.SearchTwitchStreamsAsync との DRY 原則には違反していない
    private async Task<VideoDataResponse> GetTwitchStreamsAsync()
    {
        var cacheKey = CacheKeyHelper.GetCacheKey(CacheKeyHelper.VideoType.TwitchStream);
        return await _cache.GetAsync<VideoDataResponse>(cacheKey) ?? new();
    }

    // REST API を使った解析処理
    private async Task<ChannelAnalysisResponse> GenerateAnalysisAsync(List<ProvideingVtuberData> targets)
    {
        var inputJson = JsonSerializer.Serialize(targets);
        var prompt = GetPrompt(inputJson);

        // 1. リクエストボディの作成（JSON モードを有効化）
        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
            {

                response_mime_type = "application/json" // これにより ```json 等の装飾が不要になる

            }
        };

        // 2. URL の構築
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{ModelId}:generateContent?key={_settings.ApiKey}";

        // 3. 送信（503対策：指数バックオフによるリトライ）
        HttpResponseMessage? response = null;
        var maxRetries = 3;
        for (var i = 0; i <= maxRetries; i++)
        {
            response = await _httpClient.PostAsJsonAsync(url, requestBody);

            // 503 (Overloaded) の場合は待機してリトライ
            if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                // 最大回数に達したら終了
                if (i == maxRetries)
                {
                    break;
                }

                var delaySeconds = (int)Math.Pow(2, i + 1); // 2s, 4s, 8s と増加
                //_logger.LogWarning("Geminiが混雑しています(503)。{Delay}秒後にリトライします...", delaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                continue;
            }
            break;
        }

        if (!response!.IsSuccessStatusCode)
        {
            throw new ApiServiceException("Gemini リクエスト失敗", (int)response.StatusCode);
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();

        // 4. レスポンスのパース
        using var doc = JsonDocument.Parse(jsonResponse);
        var text = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrEmpty(text))
        {
            return new();
        }

        // Markdown 装飾が含まれていたら剥ぎ取る
        var cleanedJson = text.Replace("```json", "").Replace("```", "").Trim();

        var analyses = JsonSerializer.Deserialize<List<ChannelAnalysisRaw>>(cleanedJson) ?? new();
        return new ChannelAnalysisResponse
        {
            Analyses = analyses
        };
    }

    private string GetPrompt(string inputJson)
    {
        return $$"""
             # 役割
             あなたはVTuber・ストリーマー界隈に精通し、個々の魅力を言語化するプロの紹介ライターです。
             提供された活動データ（メタデータ）を深く分析し、そのチャンネルの「個性」と「見どころ」が伝わる紹介文を作成してください。
             
             # 入力データの定義
             - "channel_id": チャンネルID
             - "video_description": 動画の説明文
             - "channel_description": チャンネルの概要欄
             - "keywords": 設定キーワード
             - "tags": カテゴリタグ
             
             # 入力データ
             {{inputJson}}
             
             # 制約事項
             - 1人あたりの文字数：400〜600文字程度。
             - トーン：親しみやすく、かつポジティブ。
             - 構成：箇条書きではなく、自然な文章で記述してください。
             - 出力形式：必ず以下のJSON配列形式のみを返してください。
             - 注意：挨拶や説明など、JSON以外の文字列は絶対に含めないでください。
             
             # 出力フォーマット
             [
               {
                 "id": "チャンネルID（入力データと同じもの）",
                 "description": "分析に基づいた魅力的な紹介文"
               }
             ]
             """;
    }
}