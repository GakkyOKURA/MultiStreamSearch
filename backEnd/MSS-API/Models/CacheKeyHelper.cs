namespace MyApi.Models;

internal static class CacheKeyHelper
{
    internal enum VideoType
    {
        YouTubeLiveStream,
        TwitchStream,
        GeminiAnakysis
    }

    internal static string GetCacheKey(VideoType type)
    {
        return type switch
        {
            VideoType.YouTubeLiveStream => "youtubeLiveStream",
            VideoType.TwitchStream => "twitchStream",
            VideoType.GeminiAnakysis => "geminiAnalysis",
            _ => throw new NotImplementedException()
        };
    }
}
