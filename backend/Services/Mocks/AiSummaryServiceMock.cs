using Google.GenAI;
using MyApi.DTOs;
using MyApi.Interfaces;
using MyApi.Models;

namespace MyApi.Services.Mocks;

public class AiSummaryServiceMock : IAiService
{
    private readonly IRedisCacheService _redisCacheService;
    public AiSummaryServiceMock(IRedisCacheService redisCacheService)
    {
        _redisCacheService = redisCacheService;
    }

    public async Task<ChannelSummaryResponse> FetchVtuberAnalysis()
    {
        var vData = await GetVtuberData();

        var raws = vData
                .Select(v => new ChannelSummaryRaw { Id = v.ChannelId, Description = "テスト" })
                .ToList();

        return new() { Analyses = raws };
    }

    private async Task<List<ProvideingVtuberData>> GetVtuberData()
    {
        var combinedList = await GetCombinedList();

        return combinedList
            .OrderBy(_ => Guid.NewGuid()) // ランダムに並び替えて...
            .Take(10) // 10 個取得
            .Select(v => new ProvideingVtuberData
            {
                ChannelId = v.ChannelId,
                VideoDescription = v.SearchDescription,
                ChannelDescription = v.ChannelDescription,
                Keywords = v.Keywords,
                Tags = v.TopicCategories
            })
            .ToList();
    }

    private async Task<List<VideoDataDTO>> GetCombinedList()
    {
        var youtubeData = await GetYouTubeLiveStreamsAsync();
        var twitchData = await GetTwitchStreamsAsync();

        return youtubeData.Items.Concat(twitchData.Items).ToList();
    }

    private async Task<VideoDataResponse> GetYouTubeLiveStreamsAsync()
    {
        var cacheKey = CacheKeyHelper.GetCacheKey(VideoType.YouTubeLiveStream);
        return await _redisCacheService.GetAsync<VideoDataResponse>(cacheKey) ?? new();
    }

    private async Task<VideoDataResponse> GetTwitchStreamsAsync()
    {
        var cacheKey = CacheKeyHelper.GetCacheKey(VideoType.TwitchStream);
        return await _redisCacheService.GetAsync<VideoDataResponse>(cacheKey) ?? new();
    }
}
