using System.Text.Json.Serialization;

namespace MyApi.Models;

public class TwitchStreamSearchResult
{
    public List<TwitchStreamSearchDto> Data { get; set; } = new();

    //[JsonPropertyName("pagination")]
    //public TwitchStreamPaginationDto? Pagination { get; set; }
}

//public class TwitchStreamPaginationDto
//{
//    [JsonPropertyName("cursor")]
//    public string? Cursor { get; set; }
//}

public class TwitchStreamSearchDto
{
    public string Id { get; set; } = "";

    public string UserId { get; set; } = "";

    public string UserLogin { get; set; } = "";

    public string UserName { get; set; } = "";

    public string GameId { get; set; } = "";

    public string GameName { get; set; } = "";

    //public string Type { get; set; } = "";

    public string Title { get; set; } = "";

    //public int ViewerCount { get; set; }

    //public DateTime StartedAt { get; set; }

    //public string Language { get; set; } = "";

    public string ThumbnailUrl { get; set; } = "";

    //public List<string>? TagIds { get; set; }

    //public bool IsMature { get; set; }
}

