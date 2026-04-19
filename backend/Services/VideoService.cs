using MyApi.DTOs;
using MyApi.Interfaces;

namespace MyApi.Services;

public class VideoService : IVideoService
{
    private readonly IYouTubeService _youTubeService;
    private readonly ITwitchService _twitchService;
    private readonly IAiService _aiService;
    public VideoService(
        IYouTubeService youTubeService,
        ITwitchService twitchService,
        IAiService aiService)
    {
        _youTubeService = youTubeService;
        _twitchService = twitchService;
        _aiService = aiService;
    }

    public async Task<VideoDataResponse> SearchVideoAsync()
    {
        var responses = await GetVideoResponses();

        var youtubeItems = responses.ytResponse.Items;
        var twitchItems = responses.twResponse.Items;

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
        var geminiResponse = await _aiService.SearchVtuberAnalysis();

        var videoResponses = await GetVideoResponses();
        var combinedList = videoResponses.ytResponse.Items.Concat(videoResponses.twResponse.Items).ToList();
        var videoDataDict = combinedList.ToDictionary(v => v.ChannelId);
        var result = new VideoWithSummaryResponse();

        foreach (var analysis in geminiResponse.Analyses)
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

    private async Task<(VideoDataResponse ytResponse, VideoDataResponse twResponse)> GetVideoResponses()
    {
        var youtubeResponse = await _youTubeService.SearchYouTubeLiveStreamsAsync();
        var twitchResponse = await _twitchService.SearchTwitchStreamsAsync();
        return(youtubeResponse, twitchResponse);
    }
}
