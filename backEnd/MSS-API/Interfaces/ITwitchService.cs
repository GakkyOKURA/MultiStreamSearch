namespace MyApi.Interfaces;

public interface ITwitchService
{
    Task<string> SearchTwitchVideosAsync(string keyword);
    Task<string> SearchCategoriesAsync(string keyword);
    Task<string> GetStreamsByCategoryAsync(string categoryId, string? cursor);
    Task<string> GetClipsByCategoryAsync(string categoryId, string period, string? cursor);
}
