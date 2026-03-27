using MyApi.Models;
using MyApi.Services;

namespace MyApi.Endpoints;

public static class VtuberDbEndPoints
{
    public static void MapVtuberEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/vtuberData");

        // vtuber 全件取得
        group.MapGet("/vtuber", async (VtuberRepository repo) =>
        {
            var result = await repo.GetAllVtubersAsync();
            return Results.Ok(result);
        });

        // vtuber 名前で前方一致検索 (api/vtubers/search?name=白)
        group.MapGet("/vtuber/name", async (string name, VtuberRepository repo) =>
        {
            if (string.IsNullOrWhiteSpace(name))
            { 
                return Results.BadRequest("検索名を入力してください"); 
            }
            var result = await repo.GetVtubersByNameAsync(name);
            return Results.Ok(result);
        });

        // vtuber group で検索
        group.MapGet("/vtuber/groupName", async (string groupName, VtuberRepository repo) =>
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                return Results.BadRequest("検索名を入力してください");
            }
            var result = await repo.GetVtubersByGroupAsync(groupName);
            return Results.Ok(result);
        });

        // vtuber name or group で検索
        group.MapGet("/vtuber/filter", async (string name, string group, VtuberRepository repo) =>
        {
            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(group))
            {
                return Results.BadRequest("検索名を入力してください");
            }
            var result = await repo.GetVtubersByNmeOrGroupAsync(name, group);
            return Results.Ok(result);
        });

        // vtuber 新規登録
        group.MapPost("/vtuber", async (VtuberDTO dto, VtuberRepository repo) =>
        {
            try
            {
                var newId = await repo.AddVtuberAsync(dto.Name, dto.GroupName, dto.PlatformName, dto.ChannelId);
                return Results.Ok(new { message = "登録完了", id = newId });
            }
            catch(Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        // vtuber 更新
        group.MapPut("/vtuber", async (VtuberDTO dto, VtuberRepository repo) =>
        {
            try
            {
                await repo.UpdateVtuberAsync(dto.Id, dto.Name, dto.GroupName, dto.PlatformName, dto.ChannelId);
                return Results.Ok(new { message = "更新完了" });
            }
            catch (Exception ex)
            {
                return Results.NotFound(ex.Message);
            }
        });

        // vtuber 削除
        group.MapDelete("/vtuber", async (int id, VtuberRepository repo) =>
        {
            try
            {
                await repo.DeleteVtuberAsync(id);
                return Results.Ok(new { message = "削除完了" });
            }
            catch (Exception ex)
            {
                return Results.NotFound(ex.Message);
            }
        });

        // group 全件取得
        group.MapGet("/group", async (VtuberRepository repo) =>
        {
            var result = await repo.GetAllGroupsAsync();
            return Results.Ok(result);
        });

        // group 新規登録
        group.MapPost("/group", async (GroupTable dto, VtuberRepository repo) =>
        {
            try
            {
                var newId = await repo.AddGroupAsync(dto.Name);
                return Results.Ok(new { message = "登録完了", id = newId });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        // group 更新
        group.MapPut("/group", async (GroupTable dto, VtuberRepository repo) =>
        {
            try
            {
                await repo.UpdateGroupAsync(dto.Id, dto.Name);
                return Results.Ok(new { message = "更新完了" });
            }
            catch (Exception ex)
            {
                return Results.NotFound(ex.Message);
            }
        });

        // group 削除
        group.MapDelete("/group", async (int id, VtuberRepository repo) =>
        {
            try
            {
                await repo.DeleteGroupAsync(id);
                return Results.Ok(new { message = "削除完了" });
            }
            catch (Exception ex)
            {
                return Results.NotFound(ex.Message);
            }
        });

        // platform 全件取得
        group.MapGet("/platform", async (VtuberRepository repo) =>
        {
            var result = await repo.GetAllPlatformsAsync();
            return Results.Ok(result);
        });

        // platform 新規登録
        group.MapPost("/platform", async (GroupTable dto, VtuberRepository repo) =>
        {
            try
            {
                var newId = await repo.AddPlatformAsync(dto.Name);
                return Results.Ok(new { message = "登録完了", id = newId });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        // platform 更新
        group.MapPut("/platform", async (GroupTable dto, VtuberRepository repo) =>
        {
            try
            {
                await repo.UpdatePlatformAsync(dto.Id, dto.Name);
                return Results.Ok(new { message = "更新完了" });
            }
            catch (Exception ex)
            {
                return Results.NotFound(ex.Message);
            }
        });

        // platform 削除
        group.MapDelete("/platform", async (int id, VtuberRepository repo) =>
        {
            try
            {
                await repo.DeletePlatformAsync(id);
                return Results.Ok(new { message = "削除完了" });
            }
            catch (Exception ex)
            {
                return Results.NotFound(ex.Message);
            }
        });
    }
}
