using System.ComponentModel.DataAnnotations; // これが必要です
using System.Text.Json.Serialization;

public class ChannelSummaryResponse
{
    [Required]
    [JsonPropertyName("analyses")] // JSON上のキー名を小文字に統一
    public List<ChannelSummaryRaw> Analyses { get; set; } = new();
}

public class ChannelSummaryRaw
{
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [Required]
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";
}