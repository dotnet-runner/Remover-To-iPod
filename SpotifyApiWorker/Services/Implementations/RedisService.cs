using SpotifyApiWorker.Services.Contracts;
using SpotifyApiWorker.ValueObjects;
using StackExchange.Redis;

namespace SpotifyApiWorker.Services.Implementations;

public class RedisService: IRedisService
{
    private readonly IDatabase _redisDbContext;
    
    public RedisService(IConnectionMultiplexer redisConnect)
    {
        _redisDbContext = redisConnect.GetDatabase();
    }
    
    public async Task<string> GetAsync(string key) =>
        await _redisDbContext.StringGetAsync(key);
    
    public Task WriteAsync(ServerSessionKey key, object value, TimeSpan? lifetime = null) =>
        lifetime is null ? _redisDbContext.StringSetAsync(key.Value, value.ToString())
            : _redisDbContext.StringSetAsync(key.Value, value.ToString(), lifetime.Value);
    
    public Task DeleteAsync(ServerSessionKey key) =>
        _redisDbContext.KeyDeleteAsync(key.Value, CommandFlags.FireAndForget);
}