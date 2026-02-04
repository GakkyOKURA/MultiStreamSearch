using MyApi.Interfaces;

namespace MyApi.Endpoints;

public static class VideoEndpoints
{
    public static void MapVideoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/videos", async (IVideoService video) =>
        {
            var result = await video.SearchVideoAsync();
            return result;
        });

        app.MapGet("/api/videos/ai", async (IVideoService video) =>
        {
            var result = await video.SearchVideoWithAnalysisAsync();
            return result;
        });
    }
}
