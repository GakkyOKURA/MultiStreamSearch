using MyApi.Models.TwitchClipSearchHelper;

namespace MyApi.Interfaces;

public interface ITwitchService
{
    Task<string> SearchTwitchVideosAsync(string keyword);
    Task<string> SearchCategoriesAsync(string keyword);
    Task<string> GetStreamsByCategoryAsync(string categoryId, string? cursor);
    Task<string> GetClipsAsync(string categoryId, string period, string? cursor);
}
