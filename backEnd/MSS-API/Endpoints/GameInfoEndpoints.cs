using MyApi.Interfaces;

namespace MyApi.Endpoints;

public static class GameInfoEndpoints
{
    public static void MapGameInfoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/gameinfos", (IGameInfoProvider gameInfo) =>
        {
            var gameInfoDto = gameInfo.GetGameInfos();
            return gameInfoDto;
        });
    }
}
