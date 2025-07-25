using System;
using course_service.Shared.Helpers;
using course_service.Shared.Interfaces;
using DotNetEnv;
using StackExchange.Redis;

namespace course_service.Shared.Services;

public class CacheManager : ICacheManager
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IDatabase _database;
    private readonly IServer _server;
    private readonly string _instanceName = "course_service_cache";
    private readonly ILogger<CacheManager> _logger;

    public CacheManager()
    {
        Env.Load();
        string redisHost = Env.GetString("REDIS_HOST");
        string redisPort = Env.GetString("REDIS_PORT");
        string redisPassword = Env.GetString("REDIS_PASSWORD");
        _connectionMultiplexer = ConnectionMultiplexer.Connect($"{redisHost}:{redisPort},password={redisPassword}");

        _database = _connectionMultiplexer.GetDatabase();
        _server = _connectionMultiplexer.GetServer(redisHost, int.Parse(redisPort));

        _logger = LoggerHelper.GetLogger<CacheManager>();
    }

    public async Task<bool?> RemoveByPattern(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new ArgumentException("Pattern cannot be null or empty", nameof(pattern));
        }

        try
        {
            const int batchSize = 1000;
            var keysToRemove = _server.KeysAsync(pattern: $"{_instanceName}:{pattern}");
            var keyBatch = new List<RedisKey>(batchSize);

            await foreach (var key in keysToRemove)
            {
                keyBatch.Add(key);

                if (keyBatch.Count >= batchSize)
                {
                    await _database.KeyDeleteAsync(keyBatch.ToArray());
                    keyBatch.Clear();
                }
            }

            // Process remaining keys in the final batch
            if (keyBatch.Count > 0)
            {
                await _database.KeyDeleteAsync(keyBatch.ToArray());
            }
            return true;
        }
        catch (RedisException ex)
        {
            _logger.LogError(ex, $"Error removing keys with pattern '{pattern}'");
            return null;
        }
    }
}
