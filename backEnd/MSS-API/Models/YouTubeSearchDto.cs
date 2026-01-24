using System.Text.Json.Serialization;

namespace MyApi.Models;

public class YouTubeSearchResult
{
    //public string Kind { get; set; } = "";

    //public string ETag { get; set; } = "";

    //public string? NextPageToken { get; set; }

    //public string? PrevPageToken { get; set; }

    //public string? RegionCode { get; set; }

    //public YouTubePageInfoDto PageInfo { get; set; } = new();

    public List<YouTubeSearchItemDto> Items { get; set; } = new();
}

//public class YouTubePageInfoDto
//{
//    public int TotalResults { get; set; }

//    public int ResultsPerPage { get; set; }
//}

public class YouTubeSearchItemDto
{
    //public string Kind { get; set; } = "";

    //public string ETag { get; set; } = "";

    public YouTubeSearchItemIdDto Id { get; set; } = new();

    public YouTubeSnippetDto Snippet { get; set; } = new();
}

public class YouTubeSearchItemIdDto
{
    //public string Kind { get; set; } = ""; // youtube#video / youtube#channel / youtube#playlist

    public string VideoId { get; set; } = "";

    public string ChannelId { get; set; } = "";

    //public string? PlaylistId { get; set; }
}

public class YouTubeSnippetDto
{
    //public DateTime PublishedAt { get; set; }

    public string ChannelId { get; set; } = "";

    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public YouTubeThumbnailsDto Thumbnails { get; set; } = new();

    public string ChannelTitle { get; set; } = "";

    //public string LiveBroadcastContent { get; set; } = "";

    //public DateTime PublishTime { get; set; }
}

public class YouTubeThumbnailsDto
{
    //public YouTubeThumbnailDto? Default { get; set; }

    public YouTubeThumbnailDto Medium { get; set; } = new();

    //public YouTubeThumbnailDto? High { get; set; }
}

public class YouTubeThumbnailDto
{
    public string Url { get; set; } = "";

    public int Width { get; set; }

    public int Height { get; set; }
}
