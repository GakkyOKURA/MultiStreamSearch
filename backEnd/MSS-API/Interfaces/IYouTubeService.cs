using MyApi.DTOs;

namespace MyApi.Interfaces;

public interface IYouTubeService
{
    Task<VideoDataResponse> SearchYouTubeLiveStreamsAsync();
    Task<VideoDataResponse> FetchYouTubeLiveStreamsAsync();
}
