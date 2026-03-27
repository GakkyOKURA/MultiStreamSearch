using MyApi.DTOs;

namespace MyApi.Interfaces;

public interface IVideoService
{
    Task<VideoDataResponse> SearchVideoAsync();
    Task<VideoWithSummaryResponse> SearchVideoWithAnalysisAsync();
}
