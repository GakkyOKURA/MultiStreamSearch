using System.Text.Json.Serialization;

namespace MyApi.Models;


public class VideoDetail
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("snippet")]
    public Snippet Snippet { get; set; }
    [JsonPropertyName("contentDetails")]
    public ContentDetails ContentDetails { get; set; }
}

public class Snippet
{
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("thumbnails")]
    public Thumbnails Thumbnails { get; set; }
}

public class Thumbnails
{
    [JsonPropertyName("default")]
    public ThumbnailInfo Default { get; set; }
    [JsonPropertyName("medium")]
    public ThumbnailInfo Medium { get; set; }
    [JsonPropertyName("high")]
    public ThumbnailInfo High { get; set; }
}

public class ThumbnailInfo
{
    [JsonPropertyName("url")]
    public string Url { get; set; }
    [JsonPropertyName("width")]
    public int Width { get; set; }
    [JsonPropertyName("height")]
    public int Height { get; set; }
}

public class ContentDetails
{
    [JsonPropertyName("duration")]
    public string Duration { get; set; } // ISO8601
}


public class YouTubeSearchResponse
{
    [JsonPropertyName("items")]
    public List<SearchItem> Items { get; set; }
}

public class DetailSearchResponse
{
    [JsonPropertyName("items")]
    public List<VideoDetail> Items { get; set; }
}

public class SearchId
{
    [JsonPropertyName("videoId")]
    public string VideoId { get; set; }
}

public class SearchItem
{
    [JsonPropertyName("id")]
    public SearchId Id { get; set; }
    [JsonPropertyName("snippet")]
    public Snippet Snippet { get; set; }
}

