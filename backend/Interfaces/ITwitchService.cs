using MyApi.DTOs;

namespace MyApi.Interfaces;

public interface ITwitchService
{
    Task<VideoDataResponse> FetchTwitchStreamsAsync();
}
