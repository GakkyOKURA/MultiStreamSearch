using MyApi.DTOs;
using MyApi.Interfaces;
using MyApi.Models;

namespace MyApi.Services.Mocks
{
    public class YouTubeServiceMock : IYouTubeService
    {
        public Task<VideoDataResponse> FetchYouTubeLiveStreamsAsync()
        {
            var dtos = new List<VideoDataDTO>();
            for (var i = 0; i < 10; i++)
            {
                var d = new VideoDataDTO
                {
                    VideoId = "ADrIqotgdyM",
                    VideoTitle = "テスト",
                    ChannelId = "UCt30jJgChL8qeT9VPadidSw",
                    ChannelName = "テスト",
                    SearchHighTumbnail = new VideoHighThumbnailDTO
                    {
                        Url = $"https://i.ytimg.com/vi/ADrIqotgdyM/hq720.jpg",
                        Width = 1280,
                        Height = 720,
                    },
                    ChannelHighThumbnail = new VideoHighThumbnailDTO
                    {
                        Url = $"https://yt3.googleusercontent.com/ytc/AIdro_m6xQ9ez0I8lnwswHqAns9ZRPsaCCutfzu6eUbM7pwzqsA=s160-c-k-c0x00ffffff-no-rj",
                        Width = 800,
                        Height = 800,
                    },
                    Platform = VIdeoPlatform.YouTube
                };

                dtos.Add(d);
            }

            var result = new VideoDataResponse()
            {
                Items = dtos,
            };

            return Task.FromResult(result);
        }
    }
}
