using MyApi.DTOs;
using MyApi.Models;

namespace MyApi.Interfaces;

public interface IYouTubeService
{
    Task<YouTubeSearchResult> SearchYouTubeLiveStreamsAsync(string keyword);
    Task<YouTubeSearchResult> SearchYouTubeShortsAsync(string keyword);
    Task<YouTubeSearchResult> FetchYouTubeLiveStreamsAsync(string keyword);
    Task<YouTubeSearchResult> FetchYouTubeShortsAsync(string keyword);
}
