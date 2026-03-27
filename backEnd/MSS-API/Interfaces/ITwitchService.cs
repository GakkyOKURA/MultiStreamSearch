using MyApi.DTOs;

namespace MyApi.Interfaces;

public interface ITwitchService
{
    Task<VideoDataResponse> SearchTwitchStreamsAsync();
    Task<VideoDataResponse> FetchTwitchStreamsAsync();
}
