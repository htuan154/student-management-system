using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using StudentManagementSystem.Services.Interfaces;

namespace StudentManagementSystem.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;

        public CacheService(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public T? GetData<T>(string key)
        {
            var jsonData = _distributedCache.GetString(key);
            return jsonData is null ? default : JsonSerializer.Deserialize<T>(jsonData);
        }

        public async Task<T?> GetDataAsync<T>(string key)
        {
            var jsonData = await _distributedCache.GetStringAsync(key);
            return jsonData is null ? default : JsonSerializer.Deserialize<T>(jsonData);
        }

        public object RemoveData(string key)
        {
            var exists = _distributedCache.GetString(key) is not null;
            if (exists)
            {
                _distributedCache.Remove(key);
            }
            return exists;
        }

        public async Task<object> RemoveDataAsync(string key)
        {
            var exists = await _distributedCache.GetStringAsync(key) is not null;
            if (exists)
            {
                await _distributedCache.RemoveAsync(key);
            }
            return exists;
        }

        // RemoveByPattern không support với Memory Cache, nên sẽ không làm gì
        public async Task RemoveByPatternAsync(string pattern)
        {
            // Memory Cache không support pattern deletion
            // Có thể log warning hoặc implement manual pattern matching nếu cần
            await Task.CompletedTask;
        }

        public bool SetData<T>(string key, T value, DateTimeOffset expirationTime)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = expirationTime
            };
            _distributedCache.SetString(key, JsonSerializer.Serialize(value), options);
            return true;
        }

        public async Task<bool> SetDataAsync<T>(string key, T value, DateTimeOffset expirationTime)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = expirationTime
            };
            await _distributedCache.SetStringAsync(key, JsonSerializer.Serialize(value), options);
            return true;
        }
    }
}
