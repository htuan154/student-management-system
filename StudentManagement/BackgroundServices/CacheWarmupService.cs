// BackgroundServices/CacheWarmupService.cs
using StudentManagementSystem.Services.Interfaces;
using StudentManagementSystem.Services;

namespace StudentManagementSystem.BackgroundServices;

public class CacheWarmupService : BackgroundService
{
    private readonly ILogger<CacheWarmupService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _warmupInterval = TimeSpan.FromHours(6);

    public CacheWarmupService(ILogger<CacheWarmupService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Initial warmup
        await WarmupCache();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_warmupInterval, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                {
                    await WarmupCache();
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
        }
    }

    private async Task WarmupCache()
    {
        try
        {
            _logger.LogInformation("Starting cache warmup...");

            using var scope = _serviceProvider.CreateScope();
            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
            var studentService = scope.ServiceProvider.GetRequiredService<IStudentService>();
            var courseService = scope.ServiceProvider.GetRequiredService<ICourseService>();

            // Warmup most accessed data
            // Preload commonly accessed data to warm up the cache
            await studentService.GetAllStudentsAsync(); // Using correct method name from interface
            await courseService.GetAllAsync();  // Using correct method name from interface

            // Warmup system cache
            // Fixed: Use DateTimeOffset instead of TimeSpan for SetDataAsync
            var expirationTime = DateTimeOffset.UtcNow.AddHours(1);
            await cacheService.SetDataAsync("system:warmup", DateTime.UtcNow, expirationTime);

            _logger.LogInformation("Cache warmup completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache warmup failed");
        }
    }
}
