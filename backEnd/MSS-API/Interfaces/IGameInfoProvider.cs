using MyApi.DTOs;

namespace MyApi.Interfaces;

public interface IGameInfoProvider
{
    GameInfoResult GetGameInfos();
}
