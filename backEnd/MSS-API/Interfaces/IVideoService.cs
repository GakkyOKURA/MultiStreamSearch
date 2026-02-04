using MyApi.Models;

namespace MyApi.Interfaces;

public interface IVideoService
{
    Task<VideoDataResponse> SearchVideoAsync();
    Task<VideoWithAnalysisResponse> SearchVideoWithAnalysisAsync();
}
