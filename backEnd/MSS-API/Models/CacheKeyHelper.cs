namespace MyApi.Models;

internal static class CacheKeyHelper
{
    internal enum VideoType
    {
        YouTubeLiveStream,
        TwitchStream,
        AiSummary
    }

    internal static string GetCacheKey(VideoType type)
    {
        return type switch
        {
            VideoType.YouTubeLiveStream => "youtubeLiveStream",
            VideoType.TwitchStream => "twitchStream",
            VideoType.AiSummary => "aiSummary",
            _ => throw new NotImplementedException()
        };
    }
}
