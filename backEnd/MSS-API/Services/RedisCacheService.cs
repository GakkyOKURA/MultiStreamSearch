using StackExchange.Redis;
using System.Text.Json;

namespace MyApi.Services;
public class RedisCacheService
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
}
