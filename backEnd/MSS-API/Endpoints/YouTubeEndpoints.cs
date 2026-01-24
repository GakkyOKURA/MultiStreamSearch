using MyApi.Interfaces;

namespace MyApi.Endpoints;

public static class YouTubeEndpoints
{
    public static void MapYouTubeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/youtube/search", async (string q, IYouTubeService yt) =>
        {
            var result = await yt.SearchYouTubeLiveStreamsAsync(q);
            return result;
        });

        app.MapGet("/api/youtube/short", async (string q, IYouTubeService yt) =>
        {
            var result = await yt.SearchYouTubeShortsAsync(q);
            return result;
        });
    }
}

