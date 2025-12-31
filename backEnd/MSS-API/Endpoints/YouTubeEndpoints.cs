using MyApi.Interfaces;

namespace MyApi.Endpoints;

public static class YouTubeEndpoints
{
    public static void MapYouTubeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/youtube/search", async (string q,string? pageToken, IYouTubeService yt) =>
        {
            var result = await yt.SearchYouTubeVideosAsync(q, pageToken);
            return Results.Content(result, "application/json");
        });

        app.MapGet("/api/youtube/short", async (string q, string period, string? pageToken, IYouTubeService yt) =>
        {
            var result = await yt.GetShortsAsync(q, period, pageToken);
            return Results.Content(result, "application/json");
        });
    }
}

