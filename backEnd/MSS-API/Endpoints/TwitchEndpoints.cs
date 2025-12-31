using MyApi.Interfaces;

namespace MyApi.Endpoints;

public static class TwitchEndpoints
{
    public static void MapTwitchEndpoints(this IEndpointRouteBuilder app)
    {
        // ★ チャンネル検索（既存）
        app.MapGet("/api/twitch/search", async (string q, ITwitchService twitch) =>
        {
            var result = await twitch.SearchTwitchVideosAsync(q);
            return Results.Content(result, "application/json");
        });

        // ★ カテゴリ検索（今回追加）
        app.MapGet("/api/twitch/categories", async (string query, ITwitchService twitch) =>
        {
            var result = await twitch.SearchCategoriesAsync(query);
            return Results.Content(result, "application/json");
        });

        // stream 検索
        app.MapGet("/api/twitch/streams", async (string categoryId, string? cursor, ITwitchService twitch) =>
        {
            var result = await twitch.GetStreamsByCategoryAsync(categoryId, cursor);
            return Results.Content(result, "application/json");
        });

        // clip 検索
        app.MapGet("/api/twitch/clips", async (string categoryId, string period, string? cursor, ITwitchService twitch) =>
        {
            var result = await twitch.GetClipsByCategoryAsync(categoryId, period, cursor);
            return Results.Content(result, "application/json");
        });
    }
}
