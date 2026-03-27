namespace MyApi.Interfaces;

public interface IRedisCacheService
{
    Task SetStringAsync(string key, string value, TimeSpan? ttl = null);
    Task<T?> GetAsync<T>(string key);
}
