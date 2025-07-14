namespace StudentManagementSystem.BackgroundServices;

public class LogCleanupService : BackgroundService
{
    private readonly ILogger<LogCleanupService> _logger;
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromDays(1);

    public LogCleanupService(ILogger<LogCleanupService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupOldLogFiles();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during log cleanup");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }
    }

    private async Task CleanupOldLogFiles()
    {
        var logDirectory = "logs";
        var maxAge = TimeSpan.FromDays(_configuration.GetValue<int>("Logging:MaxAgeDays", 30));

        if (!Directory.Exists(logDirectory))
            return;

        var files = Directory.GetFiles(logDirectory, "*.log");
        var deletedCount = 0;

        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            if (DateTime.UtcNow - fileInfo.LastWriteTimeUtc > maxAge)
            {
                try
                {
                    File.Delete(file);
                    deletedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not delete log file {File}", file);
                }
            }
        }

        if (deletedCount > 0)
        {
            _logger.LogInformation("Deleted {Count} old log files", deletedCount);
        }

        await Task.CompletedTask;
    }
}

