using Microsoft.Extensions.Options;
using MyApi.Config;
using MyApi.DTOs;
using MyApi.Interfaces;
using MyApi.Models;
using NJsonSchema.Generation;
using OpenAI;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text.Json;

namespace MyApi.Services;

public class AiSummaryService : IAiService
{
    private readonly IRedisCacheService _cache;
    private readonly HttpClient _httpClient;
    private readonly AiApiSettings _settings;
    private readonly ILogger<AiSummaryService> _logger;

    //private const string ModelId = "gemini-2.5-flash-lite";
    private const string ModelId = "gemini-3.1-flash-lite-preview";

    public AiSummaryService(
        IRedisCacheService cache,
        HttpClient httpClient, // SDK の Client から HttpClient に変更
        IOptions<AiApiSettings> settings,
        ILogger<AiSummaryService> logger)
    {
        _cache = cache;
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<ChannelSummaryResponse> SearchVtuberAnalysis()
    {
        var cacheKey = CacheKeyHelper.GetCacheKey(VideoType.AiSummary);
        return await _cache.GetAsync<ChannelSummaryResponse>(cacheKey) ?? new();
    }

    public async Task<ChannelSummaryResponse> FetchVtuberAnalysis()
    {
        var vData = await GetVtuberData();
        return await GenerateSummaryAsync_Gemini(vData);
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
        var cacheKey = CacheKeyHelper.GetCacheKey(VideoType.YouTubeLiveStream);
        return await _cache.GetAsync<VideoDataResponse>(cacheKey) ?? new();
    }

    // TwitchService.SearchTwitchStreamsAsync との DRY 原則には違反していない
    private async Task<VideoDataResponse> GetTwitchStreamsAsync()
    {
        var cacheKey = CacheKeyHelper.GetCacheKey(VideoType.TwitchStream);
        return await _cache.GetAsync<VideoDataResponse>(cacheKey) ?? new();
    }

    // テスト時は gemini の無料枠を使用
    private async Task<ChannelSummaryResponse> GenerateSummaryAsync_Gemini(List<ProvideingVtuberData> targets)
    {
        var inputJson = JsonSerializer.Serialize(targets);
        var prompt = GetPrompt(inputJson);

        var responseSchema = CreateGeminiSchema();

        // リクエストボディの作成（JSON モードを有効化）
        var requestBody = CreateGeminiBody(prompt, responseSchema);

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/" +
            $"{ModelId}:generateContent?key={_settings.GeminiApiKey}";

        var (httpResponse, msg) = await GetHttpResponseWithRetryAsync(url, requestBody);
        if (httpResponse is null)
        {
            ShowLog(msg);
            return new();
        }

        using (httpResponse)
        {
            var json = await httpResponse.Content.ReadAsStringAsync();

            // 4. レスポンスのパース
            using var doc = JsonDocument.Parse(json);
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

            var analyses = JsonSerializer.Deserialize<List<ChannelSummaryRaw>>(text) ?? new();
            return new ChannelSummaryResponse
            {
                Analyses = analyses
            };
        }
    }

    /// <summary>
    /// ai の回答が json 形式を維持するためのスキーマを作成
    /// </summary>
    /// <returns></returns>
    private object CreateGeminiSchema()
    {
        return new
        {
            type = "array", // List<ChannelSummaryRaw> なのでルートは array
            items = new
            {
                type = "object",
                properties = new
                {
                    // JsonPropertyName("id") に合わせる
                    id = new { type = "string", description = "チャンネルのユニークID" },
                    // JsonPropertyName("description") に合わせる
                    description = new { type = "string", description = "チャンネルの内容を要約した説明文" }
                },
                required = new[] { "id", "description" } // 必須項目を指定
            }
        };
    }

    /// <summary>
    /// gemini に投げる request の中身を作成
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="responseSchema"></param>
    /// <returns></returns>
    private object CreateGeminiBody(string prompt, object responseSchema)
    {
        return new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
            {
                response_mime_type = "application/json", // これにより ```json 等の装飾が不要になる
                response_schema = responseSchema
            }
        };
    }

    private async Task<(HttpResponseMessage? response, string errorMsg)> GetHttpResponseWithRetryAsync(
        string url, 
        object requestBody)
    {
        var maxRetries = 3;
        for (var tryCount = 0; tryCount <= maxRetries; tryCount++)
        {
            var response = await _httpClient.PostAsJsonAsync(url, requestBody);
            if (response.IsSuccessStatusCode)
            {
                return (response, "");
            }

            using (response)
            {
                // 503 の場合は待機してリトライ
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    if (tryCount != maxRetries)
                    {
                        var delaySeconds = (int)Math.Pow(2, tryCount + 1); // 2s, 4s, 8s と増加
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                        continue;
                    }
                }
                else if (!response.IsSuccessStatusCode)
                {
                    return (null, $"ai リクエスト {response.StatusCode} エラー");
                }
            }
        }

        return (null, "ai リクエスト 503 エラー");
    }

    // 本番運用は strict が優秀な open ai を使用
    private async Task<ChannelSummaryResponse> GenerateSummaryAsync_OpenAI(List<ProvideingVtuberData> targets)
    {
        var inputJson = JsonSerializer.Serialize(targets);
        var prompt = GetPrompt_OpenAI(inputJson);

        // 1. クライアントの初期化（リトライ設定を含む）
        // OpenAI SDKはデフォルトで指数バックオフのリトライ機能を持っている
        var clientOptions = new OpenAIClientOptions
        {
            RetryPolicy = new ClientRetryPolicy(maxRetries: 5)
        };
        var client = new ChatClient("gpt-5-mini", new ApiKeyCredential(_settings.OpenAiApiKey), clientOptions);

        var settings = new SystemTextJsonSchemaGeneratorSettings
        {
            // 全プロパティを必須にするための設定
            AllowReferencesWithProperties = false,
            DefaultReferenceTypeNullHandling = ReferenceTypeNullHandling.NotNull
        };

        var schema = NJsonSchema.JsonSchema.FromType<ChannelSummaryResponse>(settings);
        // トップレベルと、ネストされたすべてのオブジェクトで追加プロパティを禁止する
        // これにより DTO クラス側も OpenAI の Strict 制約をパスできる
        foreach (var def in schema.Definitions.Values)
        {
            def.AllowAdditionalProperties = false;
        }
        schema.AllowAdditionalProperties = false;
        schema.AllowAdditionalProperties = false;
        var schemaString = schema.ToJson();

        // 2. Structured Outputs (Strictモード) の設定
        var options = new ChatCompletionOptions()
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(

                jsonSchemaFormatName: "vtuber_summary",
                jsonSchema: BinaryData.FromString(schemaString),
                jsonSchemaFormatDescription: "Summary results for VTuber channels",
                jsonSchemaIsStrict: true
            )
        };

        // 3. 送信
        // ChatClient が 503 等のリトライを自動でハンドル
        ChatCompletion completion = await client.CompleteChatAsync(
            new List<ChatMessage> { ChatMessage.CreateUserMessage(prompt) },
            options
        );

        // 4. レスポンスの取得とパース
        // Strictモードでは Markdown の装飾（```json）は入らない
        var rawText = completion.Content[0].Text;

        if (string.IsNullOrEmpty(rawText))
        {
            return new();
        }

        // 既に ChannelAnalysisResponse の形であることが保証されているため、直接パース
        return JsonSerializer.Deserialize<ChannelSummaryResponse>(rawText) ?? new();
    }

    private string GetPrompt(string inputJson)
    {
        return $$"""
             # 役割
             あなたはVTuber・ストリーマー界隈に精通し、個々の魅力を言語化するプロの紹介ライターです。
             提供された活動データ（メタデータ）から、そのチャンネルの「個性」と「見どころ」が伝わる紹介文を作成してください。

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
                 "description": "提供データを元に作成した魅力的な紹介文"
               }
             ]
             """;
    }

    private string GetPrompt_OpenAI(string inputJson)
    {
        return $$"""
             # 役割
             あなたはVTuber・ストリーマー界隈に精通し、個々の魅力を言語化するプロの紹介ライターです。
             提供された活動データ（メタデータ）から、そのチャンネルの「個性」と「見どころ」が伝わる紹介文を作成してください。

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

             # 出力に関する補足
             - 入力に含まれるすべての id に対して、1つずつ結果を作成してください。
             - description フィールドには、提供データを元に作成した魅力的な紹介文のみを格納してください。
             """;
    }

    private void ShowLog(string message)
    {
        var time = DateTime.Now;
        _logger.LogInformation("\n{Time}{Message}\n", time, message);
    }
}