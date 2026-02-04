namespace MyApi.Models;

public class VideoWithAnalysisResponse
{
    public List<VideoWithAnalysisDTO> Items { get; set; } = new();
}
public class VideoWithAnalysisDTO : VideoDataDTO
{
    public string AiDescription { get; set; } = "";
}
