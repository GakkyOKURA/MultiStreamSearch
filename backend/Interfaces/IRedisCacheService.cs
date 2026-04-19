namespace MyApi.Interfaces;

public interface IRedisCacheService
{
    Task SetStringAsync(string key, string value, TimeSpan? ttl = null);
    Task<T?> GetAsync<T>(string key);
    Task SetVtuberFilterListAsync(string key, IEnumerable<string> channelIds);
    Task<HashSet<string>> GetVtuberFilterListAsync(string key);
    Task<string?> GetStringAsync(string key);
}
