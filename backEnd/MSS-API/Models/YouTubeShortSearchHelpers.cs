using System.Text.Json.Serialization;

namespace MyApi.Models.YouTubeShortSearchHelper;

public class YouTubeSearchResponse
{
    [JsonPropertyName("items")]
    public List<SearchItem> Items { get; set; } = new();
}

public class SearchItem
{
    [JsonPropertyName("id")]
    public SearchId Id { get; set; } = new();

    [JsonPropertyName("snippet")]
    public Snippet Snippet { get; set; } = new();
}

public class SearchId
{
    [JsonPropertyName("videoId")]
    public string VideoId { get; set; } = "";
}

public class Snippet
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("channelTitle")]
    public string ChannnelTitle { get; set; } = "";

    [JsonPropertyName("thumbnails")]
    public Thumbnails Thumbnails { get; set; } = new();
}

public class Thumbnails
{
    [JsonPropertyName("default")]
    public ThumbnailInfo Default { get; set; } = new();

    [JsonPropertyName("medium")]
    public ThumbnailInfo Medium { get; set; } = new();

    [JsonPropertyName("high")]
    public ThumbnailInfo High { get; set; } = new();
}

public class ThumbnailInfo
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = "";

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }
}