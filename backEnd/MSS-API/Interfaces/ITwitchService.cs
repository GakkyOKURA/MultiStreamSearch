using MyApi.Models;

namespace MyApi.Interfaces;

public interface ITwitchService
{
    Task<TwitchStreamSearchResult> SearchTwitchStreamsAsync(string gameId);
    Task<TwitchStreamSearchResult> FetchTwitchStreamsAsync(string gameId);
    Task<TwitchClipSearchResult> SearchTwitchClipsAsync(string gameId);
    Task<TwitchClipSearchResult> FetchTwitchClipsAsync(string gameId);
}
