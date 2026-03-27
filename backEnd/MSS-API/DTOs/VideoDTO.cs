using MyApi.Models;
using System.Text.Json.Serialization;

namespace MyApi.DTOs;

public class VideoDataResponse
{
    public List<VideoDataDTO> Items { get; set; } = new();
}

public class VideoDataDTO
{
    public string VideoId { get; set; } = "";

    public string VideoTitle { get; set; } = ""; // AI に読ませる

    public string ChannelId { get; set; } = "";

    public string ChannelName { get; set; } = "";

    public string SearchDescription { get; set; } = ""; // AI に読ませる

    public string ChannelDescription { get; set; } = ""; // AI に読ませる

    public VideoHighThumbnailDTO SearchHighTumbnail { get; set; } = new();

    public VideoHighThumbnailDTO ChannelHighThumbnail { get; set; } = new();

    public string Keywords { get; set; } = ""; // AI に読ませる

    public string BannerExternalUrl { get; set; } = "";

    public List<string> TopicCategories { get; set; } = new(); // AI に読ませる

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public VIdeoPlatform Platform { get; set; }
}

public class VideoHighThumbnailDTO
{
    public string Url { get; set; } = "";

    public int Width { get; set; }

    public int Height { get; set; }
}
