namespace MyApi.Models;

internal static class CacheKeyHelper
{
    internal enum VideoType
    {
        YouTubeLiveStream,
        YouTubeShort,
        TwitchStream,
        TwitchClip
    }

    internal static string GetCacheKey(VideoType type, string param)
    {
        return type switch
        {
            VideoType.YouTubeLiveStream => $"youtubeLiveStream:{param}",
            VideoType.YouTubeShort => $"youtubeShort:{param}",
            VideoType.TwitchStream => $"twitchStream:{param}",
            VideoType.TwitchClip => $"twitchClip:{param}",
            _ => throw new NotImplementedException()
        };
    }
}
