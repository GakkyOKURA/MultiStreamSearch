namespace MyApi.DTOs;

public class YouTubeSearchResponseDto
{
    //必要なら後で追加
}

public class YouTubeVideoDetailDto
{
    public string VideoId { get; set; }
    public string Title { get; set; }
    public string ThumbnailUrl { get; set; }
    public string ChannelTitle { get; set; }
    public string PublishedAt { get; set; }
    public long ViewCount { get; set; }
    public long LikeCount { get; set; }
    public long CommentCount { get; set; }
    public string Duration { get; set; }
}
