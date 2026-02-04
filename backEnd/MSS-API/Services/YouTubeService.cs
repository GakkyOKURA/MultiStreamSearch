using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;
using MyApi.Config;
using MyApi.Interfaces;
using MyApi.Models;
using MyApi.Models.Channels;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using MyApi.CutomException;

namespace MyApi.Services;

public class YouTubeService : IYouTubeService
{
    private readonly RedisCacheService _cache;
    private readonly HttpClient _httpClient;
    private readonly YouTubeApiSettings _settings;
    

    public YouTubeService(RedisCacheService cache, HttpClient httpClient, IOptions<YouTubeApiSettings> settings)
    {
        _cache = cache;
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<VideoDataResponse> SearchYouTubeLiveStreamsAsync()
    {
        var cacheKey = CacheKeyHelper.GetCacheKey(CacheKeyHelper.VideoType.YouTubeLiveStream);
        return await _cache.GetAsync<VideoDataResponse>(cacheKey) ?? new();
    }

    //チャンネル名とタイトルどちらかに日本語が入っていれば日本語の動画判定とする
    //クォータに余裕があれば search video で判定する
    private List<YouTubeSearchItemRaw> SortJapaneseFirst(List<YouTubeSearchItemRaw> items)
    {
        return items
            .OrderByDescending(v =>
                HasJapanese(v.Snippet.Title) || HasJapanese(v.Snippet.ChannelTitle))
            .ToList();
    }

    private static bool HasJapanese(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;

        // ひらがな・カタカナ・漢字
        return Regex.IsMatch(text, @"[\u3040-\u30FF\u4E00-\u9FFF]");
    }

    public async Task<VideoDataResponse> FetchYouTubeLiveStreamsAsync()
    {
        var searchResponse = await GetYouTubeLiveStreamsAsync();
        if(searchResponse.Items.Count == 0)
        {
            return new VideoDataResponse();
        }

        var channelResponse = await GetChannelInformationAsync(searchResponse.Items);
        var dto = ToDTO(searchResponse.Items, channelResponse.Items);
        dto.Items = dto.Items
            .Where(v => !IndividualChecker.IsCompany(v.ChannelDescription))
            .ToList();
        return dto;
    }

    private async Task<YouTubeSearchResponse> GetYouTubeLiveStreamsAsync()
    {
        var allResponse = new YouTubeSearchResponse();

        var baseUrl =
            "https://www.googleapis.com/youtube/v3/search" +
            $"?part=snippet" +
            $"&type=video" +
            $"&eventType=live" +
            $"&regionCode=JP" +
            $"&relevanceLanguage=ja" +
            $"&maxResults=50" +
            $"&q=Vtuber" +
            $"&key={_settings.ApiKey}";

        var nextPageToken = "";
        var searchCount = 2; //今後 api 制限が緩和したら増やす
        for (var i = 0; i < searchCount; i++)
        {
            var url = baseUrl;

            if(!string.IsNullOrEmpty(nextPageToken))
            {
                url += $"&pageToken={nextPageToken}";
            }

            var httpResponse = await _httpClient.GetAsync(url);
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new ApiServiceException("Search.list リクエスト失敗", (int)httpResponse.StatusCode);
            }

            var json = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<YouTubeSearchResponse>(json);
            if (response is null)
            {
                return new();
            }

            allResponse.Items.AddRange(response.Items);
            // 完全に重複を除く
            allResponse.Items = allResponse.Items
                .DistinctBy(v => v.Id.VideoId)
                .DistinctBy(v => v.Id.ChannelId)
                .ToList();
            if(string.IsNullOrEmpty(response.NextPageToken))
            {
                break;
            }
            nextPageToken = response.NextPageToken;
        }

        return allResponse;
    }

    private async Task<YouTubeChannelsResponse> GetChannelInformationAsync(List<YouTubeSearchItemRaw> items)
    {
        var channelIdBatches = items
            .Select(x => x.Snippet.ChannelId)
            .Chunk(50)
            .ToList();

        var allResults = new YouTubeChannelsResponse();

        foreach (var batch in channelIdBatches)
        {
            var ids = string.Join(",", batch);

            var url =
                $"https://www.googleapis.com/youtube/v3/channels" +
                $"?part=snippet,brandingSettings,topicDetails" +
                $"&id={ids}" +
                $"&key={_settings.ApiKey}";

            var httpResponse = await _httpClient.GetAsync(url);
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new ApiServiceException($"Channels.list リクエスト失敗", (int)httpResponse.StatusCode);
            }

            var json = await httpResponse.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<YouTubeChannelsResponse>(json);

            if (result?.Items is not null)
            {
                allResults.Items.AddRange(result.Items);
            }
        }

        return allResults;
    }

    private VideoDataResponse ToDTO(List<YouTubeSearchItemRaw> sItems, List<YouTubeChannelItemRaw> cItems)
    {
        return new VideoDataResponse
        {
            Items = sItems
                .Join(
                    cItems,
                    s => s.Snippet.ChannelId,
                    c => c.Id,
                    (s, c) => new VideoDataDTO
                    {
                        VideoId = s.Id.VideoId,
                        VideoTitle = s.Snippet.Title,
                        ChannelId = s.Snippet.ChannelId,
                        ChannelName = s.Snippet.ChannelTitle,
                        SearchDescription = s.Snippet.Description,
                        ChannelDescription = c.Snippet.Description,
                        SearchHighTumbnail = new VideoHighThumbnailDTO
                        {
                            Url = $"https://i.ytimg.com/vi/{s.Id.VideoId}/hq720.jpg",//s.Snippet.Thumbnails.High.Url,
                            Width = 1280,//s.Snippet.Thumbnails.High.Width,
                            Height = 720,//s.Snippet.Thumbnails.High.Height,
                        },
                        ChannelHighThumbnail = new VideoHighThumbnailDTO
                        {
                            Url = c.Snippet.Thumbnails.High.Url,
                            Width = c.Snippet.Thumbnails.High.Width,
                            Height = c.Snippet.Thumbnails.High.Height
                        },
                        Keywords = c.BrandingSettings.Channel.Keywords,
                        BannerExternalUrl = c.BrandingSettings.Image.BannerExternalUrl,
                        TopicCategories = c.TopicDetails.TopicCategories
                            .Select
                            (
                                // AI と人間が読みやすいように アンダーバー、特殊文字を修正
                                url => System.Net.WebUtility.UrlDecode(
                                url.Split('/')
                                .Last()
                                .Replace("_", " "))
                            )
                            .ToList(),
                        Platform = VIdeoPlatform.YouTube
                    }
                )
                .ToList()
        };
    }
}

