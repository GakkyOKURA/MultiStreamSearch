using MyApi.DTOs;
using MyApi.Interfaces;
using MyApi.Models;

namespace MyApi.Services;

public class VideoService : IVideoService
{
    private readonly IRedisCacheService _redisCacheService;

    public VideoService(IRedisCacheService redisCacheService)
    {
        _redisCacheService = redisCacheService;
    }

    public async Task<VideoDataResponse> SearchVideoAsync()
    {
        //var responses = await GetVideoResponses();

        var youtubeResponse = await GetYouTubeFromCache();
        var twitchResponse = await GetTwitchFromCache();

        var youtubeItems = youtubeResponse.Items;
        var twitchItems = twitchResponse.Items;

        var combinedItems = new List<VideoDataDTO>();
        var maxCount = Math.Max(youtubeItems.Count, twitchItems.Count);

        for (var i = 0; i < maxCount; i++)
        {
            if (i < youtubeItems.Count)
            {
                combinedItems.Add(youtubeItems[i]);
            }

            if (i < twitchItems.Count)
            {
                combinedItems.Add(twitchItems[i]);
            }
        }

        return new VideoDataResponse
        {
            Items = combinedItems
        };
    }

    public async Task<VideoWithSummaryResponse> SearchVideoWithAnalysisAsync()
    {
        //var aiResponse = await _aiService.SearchVtuberAnalysis();
        var aiResponse = await GetAiSummaryFromCache();

        var youtubeResponse = await GetYouTubeFromCache();
        var twitchResponse = await GetTwitchFromCache();

        //var videoResponses = await GetVideoResponses();
        var combinedList = youtubeResponse.Items
            .Concat(twitchResponse.Items)
            .ToList();
        var videoDataDict = combinedList
            .DistinctBy(v => v.ChannelId)
            .ToDictionary(v => v.ChannelId);
        var result = new VideoWithSummaryResponse();

        foreach (var analysis in aiResponse.Analyses)
        {
            if (videoDataDict.TryGetValue(analysis.Id, out var data))
            {
                result.Items.Add(new VideoWithSummaryDTO
                {
                    VideoId = data.VideoId,
                    VideoTitle = data.VideoTitle,
                    ChannelId = data.ChannelId,
                    ChannelName = data.ChannelName,
                    SearchDescription = data.SearchDescription,
                    ChannelDescription = data.ChannelDescription,
                    SearchHighTumbnail = data.SearchHighTumbnail,
                    ChannelHighThumbnail = data.ChannelHighThumbnail,
                    Keywords = data.Keywords,
                    BannerExternalUrl = data.BannerExternalUrl,
                    TopicCategories = data.TopicCategories,
                    Platform = data.Platform,
                    AiDescription = analysis.Description
                });

                if (result.Items.Count >= 10)
                {
                    break;
                }
            }
        }

        return result;
    }

    //private async Task<(VideoDataResponse ytResponse, VideoDataResponse twResponse)> GetVideoResponses()
    //{
    //    var youtubeResponse = await _youTubeService.SearchYouTubeLiveStreamsAsync();
    //    var twitchResponse = await _twitchService.SearchTwitchStreamsAsync();
    //    return(youtubeResponse, twitchResponse);
    //}

    private async Task<VideoDataResponse> GetYouTubeFromCache()
    {
        var cacheKey = CacheKeyHelper.GetCacheKey(VideoType.YouTubeLiveStream);
        return await _redisCacheService.GetAsync<VideoDataResponse>(cacheKey) ?? new();
    }

    private async Task<VideoDataResponse> GetTwitchFromCache()
    {
        var cacheKey = CacheKeyHelper.GetCacheKey(VideoType.TwitchStream);
        return await _redisCacheService.GetAsync<VideoDataResponse>(cacheKey) ?? new();
    }

    private async Task<ChannelSummaryResponse> GetAiSummaryFromCache()
    {
        var cacheKey = CacheKeyHelper.GetCacheKey(VideoType.AiSummary);
        return await _redisCacheService.GetAsync<ChannelSummaryResponse>(cacheKey) ?? new();
    }
}