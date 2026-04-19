namespace MyApi.Models;

internal static class CacheKeyHelper
{
    internal static string GetTwitchTokenCacheKey()
    {
        return "twitchAccessToken";
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

    internal static string GetVtuberCachKey(VIdeoPlatform platform)
    {
        return platform switch
        {
            VIdeoPlatform.YouTube => "vtuberYouTube",
            VIdeoPlatform.Twitch => "vtuberTwitch",
            _ => throw new NotImplementedException()
        };
    }
}
