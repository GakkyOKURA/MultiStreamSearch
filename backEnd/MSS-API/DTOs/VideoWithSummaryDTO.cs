namespace MyApi.DTOs;

public class VideoWithSummaryResponse
{
    public List<VideoWithSummaryDTO> Items { get; set; } = new();
}
public class VideoWithSummaryDTO : VideoDataDTO
{
    public string AiDescription { get; set; } = "";
}
