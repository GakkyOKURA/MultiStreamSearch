using MyApi.DTOs;
using MyApi.Interfaces;
using MyApi.Models;

namespace MyApi.Services.Mocks;

public class TwitchServiceMock : ITwitchService
{
    public Task<VideoDataResponse> FetchTwitchStreamsAsync()
    {
        var dtos = new List<VideoDataDTO>();
        for (var i = 0; i < 10; i++)
        {
            var d = new VideoDataDTO
            {
                VideoId = "",
                VideoTitle = "テスト",
                ChannelId = "akamikarubi",
                ChannelName = "テスト",
                SearchHighTumbnail = new VideoHighThumbnailDTO
                {
                    Url = $"https://static-cdn.jtvnw.net/previews-ttv/live_user_ukyochi_jp-1280x720.jpg",
                    Width = 1280,
                    Height = 720,
                },
                ChannelHighThumbnail = new VideoHighThumbnailDTO
                {
                    Url = $"https://static-cdn.jtvnw.net/jtv_user_pictures/f5ba0ca0-2187-41ea-b7bb-d0457b1dba0e-profile_image-70x70.png",
                    Width = 300,
                    Height = 300,
                },
                Platform = VIdeoPlatform.Twitch
            };

            dtos.Add(d);
        }

        var  result = new VideoDataResponse()
        {
            Items = dtos,
        };

        return Task.FromResult(result);
    }
}
