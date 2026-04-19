using MyApi.Models;
using Npgsql;

namespace MyApi.Interfaces;

public interface IVtuberRepository
{
    Task InitializeDatabaseAsync();
    Task<VtuberResponse> GetAllVtubersAsync();
    Task<VtuberResponse> GetVtubersByNameAsync(string searchName);
    Task<VtuberResponse> GetVtubersByGroupAsync(string groupName);
    Task<VtuberResponse> GetVtubersByFilterAsync(string searchName, string groupName, string platform);
    Task<int> AddVtuberAsync(string vtuberName, string groupName, string platformName, string channelId);
    Task UpdateVtuberAsync(int id, string newName, string newGroupName, string newPlatformName, string newChannelId);
    Task DeleteVtuberAsync(int id);
    Task<GroupResponse> GetAllGroupsAsync();
    Task<int> AddGroupAsync(string groupName);
    Task UpdateGroupAsync(int id, string newName);
    Task DeleteGroupAsync(int id);
    Task<PlatformResponse> GetAllPlatformsAsync();
    Task<int> AddPlatformAsync(string platformName);
    Task UpdatePlatformAsync(int id, string newName);
    Task DeletePlatformAsync(int id);
    Task IncrementAsync();
    Task<long> GetCountAsync();
}
