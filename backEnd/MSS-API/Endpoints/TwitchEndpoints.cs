using MyApi.Interfaces;

namespace MyApi.Endpoints;

public static class TwitchEndpoints
{
    public static void MapTwitchEndpoints(this IEndpointRouteBuilder app)
    {
        // チャンネル検索
        // 将来的に使用
        //app.MapGet("/api/twitch/search", async (string q, ITwitchService twitch) =>
        //{
        //    var result = await twitch.SearchTwitchVideosAsync(q);
        //    return Results.Content(result, "application/json");
        //});

        // カテゴリ検索
        // 将来的に使用
        //app.MapGet("/api/twitch/categories", async (string query, ITwitchService twitch) =>
        //{
        //    var result = await twitch.SearchCategoriesAsync(query);
        //    return Results.Content(result, "application/json");
        //});

        // stream 検索
        app.MapGet("/api/twitch/streams", async (string gameId, ITwitchService twitch) =>
        {
            var result = await twitch.SearchTwitchStreamsAsync(gameId);
            return result;
        });

        // clip 検索
        app.MapGet("/api/twitch/clips", async (string gameId, ITwitchService twitch) =>
        {
            var result = await twitch.SearchTwitchClipsAsync(gameId);
            return result;
        });
    }
}
