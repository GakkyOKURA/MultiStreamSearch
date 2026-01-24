namespace MyApi.DTOs;

public class GameInfoResult
{
    public List<GameInfoDto> GameInfos { get; set; } = new();
}

public class GameInfoDto
{
    public string GameName { get; set; } = "";
    public string GameId { get; set; } = "";
    public string BoxArtTemplateUrl { get; set; } = "";
}
