using MyApi.DTOs;

namespace MyApi.Interfaces;

public interface IYouTubeService
{
    Task<string> SearchYouTubeVideosAsync(string keyword, string? pageToken);
    Task<string> GetShortsAsync(string keyword, string period, string? pageToken);
    //Task<List<YouTubeVideoDetailDto>> SearchVideosWithDetailsAsync(string keyword);

}
