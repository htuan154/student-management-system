// BackgroundServices/CacheCleanupService.cs
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using StudentManagementSystem.Services.Interfaces;
namespace StudentManagementSystem.BackgroundServices;

public class CacheCleanupService : BackgroundService
{
    private readonly ILogger<CacheCleanupService> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1);

    public CacheCleanupService(
        ILogger<CacheCleanupService> logger,
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Starting cache cleanup at {Time}", DateTime.UtcNow);

                await CleanupExpiredCacheEntries();
                await CompactMemoryCache();

                _logger.LogInformation("Cache cleanup completed at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cache cleanup");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }
    }

    private async Task CleanupExpiredCacheEntries()
    {
        using var scope = _serviceProvider.CreateScope();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

        // Cleanup expired entries using cache service
        await cacheService.RemoveByPatternAsync("temp:*");
        await cacheService.RemoveByPatternAsync("short_term:*");
    }

    private Task CompactMemoryCache()
    {
        if (_memoryCache is MemoryCache mc)
        {
            mc.Compact(0.25); // Remove 25% of entries
        }
        return Task.CompletedTask;
    }
}
