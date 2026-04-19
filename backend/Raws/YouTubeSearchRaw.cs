using System.Text.Json.Serialization;

namespace MyApi.Raws.Search;

public class YouTubeSearchResponse
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "";

    [JsonPropertyName("etag")]
    public string ETag { get; set; } = "";

    [JsonPropertyName("nextPageToken")]
    public string NextPageToken { get; set; } = "";

    [JsonPropertyName("prevPageToken")]
    public string PrevPageToken { get; set; } = "";

    [JsonPropertyName("regionCode")]
    public string RegionCode { get; set; } = "";

    [JsonPropertyName("pageInfo")]
    public YouTubePageInfoRaw PageInfo { get; set; } = new();

    [JsonPropertyName("items")]
    public List<YouTubeSearchItemRaw> Items { get; set; } = new();
}

public class YouTubePageInfoRaw
{
    [JsonPropertyName("totalResults")]
    public int TotalResults { get; set; }

    [JsonPropertyName("resultsPerPage")]
    public int ResultsPerPage { get; set; }
}

public class YouTubeSearchItemRaw
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "";

    [JsonPropertyName("etag")]
    public string ETag { get; set; } = "";

    [JsonPropertyName("id")]
    public YouTubeSearchItemIdRaw Id { get; set; } = new();

    [JsonPropertyName("snippet")]
    public YouTubeSnippetRaw Snippet { get; set; } = new();
}

public class YouTubeSearchItemIdRaw
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = ""; // youtube#video / youtube#channel / youtube#playlist

    [JsonPropertyName("videoId")]
    public string VideoId { get; set; } = "";

    [JsonPropertyName("channelId")]
    public string ChannelId { get; set; } = "";

    [JsonPropertyName("playlistId")]
    public string PlaylistId { get; set; } = "";
}

public class YouTubeSnippetRaw
{
    [JsonPropertyName("publishedAt")]
    public DateTime PublishedAt { get; set; }

    [JsonPropertyName("channelId")]
    public string ChannelId { get; set; } = "";

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("thumbnails")]
    public YouTubeThumbnailsRaw Thumbnails { get; set; } = new();

    [JsonPropertyName("channelTitle")]
    public string ChannelTitle { get; set; } = "";

    [JsonPropertyName("liveBroadcastContent")]
    public string LiveBroadcastContent { get; set; } = "";

    [JsonPropertyName("publishTime")]
    public DateTime PublishTime { get; set; }
}

public class YouTubeThumbnailsRaw
{
    [JsonPropertyName("default")]
    public YouTubeThumbnailRaw Default { get; set; } = new();

    [JsonPropertyName("medium")]
    public YouTubeThumbnailRaw Medium { get; set; } = new();

    [JsonPropertyName("high")]
    public YouTubeThumbnailRaw High { get; set; } = new();
}

public class YouTubeThumbnailRaw
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = "";

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }
}
