using MyApi.DTOs;
using MyApi.Interfaces;

namespace MyApi.Models;

public class GameInfoProvider : IGameInfoProvider
{
    public GameInfoResult GetGameInfos()
    {
        var dto = new GameInfoResult
        {
            GameInfos = SearchWordHelper.GameIds
                .Select(kv => new GameInfoDto
                {
                    GameName = kv.Key,
                    GameId = kv.Value
                })
                .ToList()
        };

        return dto;
    }
}

