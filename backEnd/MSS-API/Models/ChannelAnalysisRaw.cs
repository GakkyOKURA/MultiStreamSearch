using System.Text.Json.Serialization;

namespace MyApi.Models;

public class ChannelAnalysisResponse
{
    public List<ChannelAnalysisRaw> Analyses { get; set; } = new();
}
public class ChannelAnalysisRaw
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";
}
