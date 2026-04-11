using StackExchange.Redis;
using System.Text.Json;
using MyApi.Interfaces;
using MyApi.Models;

namespace MyApi.Services;

public class RedisCacheService : IRedisCacheService
{
    private readonly IDatabase _db;

    public RedisCacheService(IConnectionMultiplexer mux)
    {
        _db = mux.GetDatabase();
    }

    public async Task SetStringAsync(string key, string value, TimeSpan? ttl = null)
    {
        await _db.StringSetAsync(key, value, ttl);
    }

    public async Task<string?> GetStringAsync(string key)
    {
        return await _db.StringGetAsync(key);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var json = await _db.StringGetAsync(key);
        if (json.IsNullOrEmpty)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(json!);
    }

    public async Task SetVtuberFilterListAsync(string key, IEnumerable<string> channelIds)
    {
        var idsRedisValue = channelIds
            .Select(v => (RedisValue)v)
            .ToArray();

        // 先に削除。 SetAdd は StringSet と違って上書きではなく追加なので、一度削除する必要がある
        await _db.KeyDeleteAsync(key);
        await _db.SetAddAsync(key, idsRedisValue);
    }

    public async Task<HashSet<string>> GetVtuberFilterListAsync(string key)
    {
        var filter = await _db.SetMembersAsync(key);
        return new HashSet<string>(filter.Select(v => v.ToString()));
    }
}