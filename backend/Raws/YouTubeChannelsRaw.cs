using System.Text.Json.Serialization;

namespace MyApi.Raws.Channel;

public class YouTubeChannelsResponse
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "";

    [JsonPropertyName("etag")]
    public string Etag { get; set; } = "";

    [JsonPropertyName("pageInfo")]
    public YouTubePageInfoRaw PageInfo { get; set; } = new();

    [JsonPropertyName("nextPageToken")]
    public string NextPageToken { get; set; } = "";

    [JsonPropertyName("prevPageToken")]
    public string PrevPageToken { get; set; } = "";

    [JsonPropertyName("items")]
    public List<YouTubeChannelItemRaw> Items { get; set; } = new();
}

// ----------------------
// pageInfo
// ----------------------
public class YouTubePageInfoRaw
{
    [JsonPropertyName("totalResults")]
    public int TotalResults { get; set; }

    [JsonPropertyName("resultsPerPage")]
    public int ResultsPerPage { get; set; }
}

// ----------------------
// items[] の中身
// ----------------------
public class YouTubeChannelItemRaw
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "";

    [JsonPropertyName("etag")]
    public string Etag { get; set; } = "";

    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("snippet")]
    public YouTubeSnippetRaw Snippet { get; set; } = new();

    [JsonPropertyName("brandingSettings")]
    public YouTubeBrandingSettingsRaw BrandingSettings { get; set; } = new();

    [JsonPropertyName("topicDetails")]
    public TopicDetailsRaw TopicDetails { get; set; } = new();
}

// ----------------------
// snippet
// ----------------------
public class YouTubeSnippetRaw
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("customUrl")]
    public string CustomUrl { get; set; } = "";

    [JsonPropertyName("publishedAt")]
    public DateTime PublishedAt { get; set; }

    [JsonPropertyName("thumbnails")]
    public YouTubeThumbnailsRaw Thumbnails { get; set; } = new();

    [JsonPropertyName("country")]
    public string Country { get; set; } = "";
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

// ----------------------
// brandingSettings
// ----------------------
public class YouTubeBrandingSettingsRaw
{
    [JsonPropertyName("channel")]
    public YouTubeBrandingChannelRaw Channel { get; set; } = new();

    [JsonPropertyName("image")]
    public YouTubeBrandingImageRaw Image { get; set; } = new();
}

public class YouTubeBrandingChannelRaw
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("keywords")]
    public string Keywords { get; set; } = "";

    [JsonPropertyName("defaultLanguage")]
    public string DefaultLanguage { get; set; } = "";

    [JsonPropertyName("country")]
    public string Country { get; set; } = "";

    [JsonPropertyName("featuredChannelsUrls")]
    public List<string> FeaturedChannelsUrls { get; set; } = new();
}

public class YouTubeBrandingImageRaw
{
    [JsonPropertyName("bannerExternalUrl")]
    public string BannerExternalUrl { get; set; } = "";
}

public class TopicDetailsRaw
{
    [JsonPropertyName("topicIds")]
    public List<string> TopicIds { get; set; } = new();

    [JsonPropertyName("topicCategories")]
    public List<string> TopicCategories { get; set; } = new();
}

