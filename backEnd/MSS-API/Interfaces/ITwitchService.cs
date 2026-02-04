using MyApi.Models;

namespace MyApi.Interfaces;

public interface ITwitchService
{
    Task<VideoDataResponse> SearchTwitchStreamsAsync();
    Task<VideoDataResponse> FetchTwitchStreamsAsync();
}
