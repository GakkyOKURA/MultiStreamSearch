using System.Text.Json.Serialization;

namespace MyApi.Models;

public class ProvideingVtuberData
{
    [JsonPropertyName("channel_id")]
    public string ChannelId { get; set; } = "";

    [JsonPropertyName("video_description")]
    public string VideoDescription { get; set; } = "";

    [JsonPropertyName("channel_description")]
    public string ChannelDescription { get; set; } = "";

    [JsonPropertyName("keywords")]
    public string Keywords { get; set; } = "";

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new List<string>();

}
