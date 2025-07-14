// BackgroundServices/MetricsCollectionService.cs
using StudentManagementSystem.Services.Interfaces;
using System.Diagnostics;

namespace StudentManagementSystem.BackgroundServices;

public class MetricsCollectionService : BackgroundService
{
    private readonly ILogger<MetricsCollectionService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _collectionInterval = TimeSpan.FromMinutes(5); // Collect metrics every 5 minutes

    public MetricsCollectionService(ILogger<MetricsCollectionService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Metrics Collection Service started");

        // Initial collection
        await CollectMetrics();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_collectionInterval, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                {
                    await CollectMetrics();
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
        }

        _logger.LogInformation("Metrics Collection Service stopped");
    }

    private async Task CollectMetrics()
    {
        try
        {
            _logger.LogInformation("Starting metrics collection...");

            using var scope = _serviceProvider.CreateScope();
            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
            var studentService = scope.ServiceProvider.GetRequiredService<IStudentService>();
            var courseService = scope.ServiceProvider.GetRequiredService<ICourseService>();

            // Collect system metrics
            var systemMetrics = await CollectSystemMetrics();

            // Collect application metrics
            var appMetrics = await CollectApplicationMetrics(studentService, courseService);

            // Store metrics in cache for dashboard/reporting
            var metricsData = new
            {
                Timestamp = DateTime.UtcNow,
                SystemMetrics = systemMetrics,
                ApplicationMetrics = appMetrics
            };

            // Store current metrics
            await cacheService.SetDataAsync("metrics:current", metricsData, DateTimeOffset.UtcNow.AddMinutes(10));

            // Store historical metrics (keep for 24 hours)
            var historicalKey = $"metrics:historical:{DateTime.UtcNow:yyyyMMddHHmm}";
            await cacheService.SetDataAsync(historicalKey, metricsData, DateTimeOffset.UtcNow.AddHours(24));

            _logger.LogInformation("Metrics collection completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Metrics collection failed");
        }
    }

    private async Task<object> CollectSystemMetrics()
    {
        var process = Process.GetCurrentProcess();

        return new
        {
            // Memory usage
            WorkingSet = process.WorkingSet64,
            PrivateMemory = process.PrivateMemorySize64,
            VirtualMemory = process.VirtualMemorySize64,

            // CPU usage
            ProcessorTime = process.TotalProcessorTime,
            ThreadCount = process.Threads.Count,

            // System info
            MachineName = Environment.MachineName,
            ProcessorCount = Environment.ProcessorCount,
            OSVersion = Environment.OSVersion.ToString(),

            // GC info
            Gen0Collections = GC.CollectionCount(0),
            Gen1Collections = GC.CollectionCount(1),
            Gen2Collections = GC.CollectionCount(2),
            TotalMemory = GC.GetTotalMemory(false)
        };
    }

    private async Task<object> CollectApplicationMetrics(IStudentService studentService, ICourseService courseService)
    {
        try
        {
            // Get total counts
            var allStudents = await studentService.GetAllStudentsAsync();
            var allCourses = await courseService.GetAllAsync();
            var activeCourses = await courseService.GetActiveCoursesAsync();

            var studentCount = allStudents.Count();
            var courseCount = allCourses.Count();
            var activeCourseCount = activeCourses.Count();

            // Calculate enrollment metrics
            var totalEnrollments = 0;
            var courseEnrollments = new Dictionary<string, int>();

            foreach (var course in allCourses)
            {
                var enrollmentCount = await courseService.GetEnrollmentCountInCourseAsync(course.CourseId);
                totalEnrollments += enrollmentCount;
                courseEnrollments[course.CourseId] = enrollmentCount;
            }

            // Calculate averages
            var averageEnrollmentPerCourse = courseCount > 0 ? (double)totalEnrollments / courseCount : 0;
            var averageEnrollmentPerStudent = studentCount > 0 ? (double)totalEnrollments / studentCount : 0;

            // Find most popular courses
            var mostPopularCourses = courseEnrollments
                .OrderByDescending(x => x.Value)
                .Take(5)
                .ToDictionary(x => x.Key, x => x.Value);

            return new
            {
                // Basic counts
                TotalStudents = studentCount,
                TotalCourses = courseCount,
                ActiveCourses = activeCourseCount,
                InactiveCourses = courseCount - activeCourseCount,

                // Enrollment metrics
                TotalEnrollments = totalEnrollments,
                AverageEnrollmentPerCourse = Math.Round(averageEnrollmentPerCourse, 2),
                AverageEnrollmentPerStudent = Math.Round(averageEnrollmentPerStudent, 2),

                // Popular courses
                MostPopularCourses = mostPopularCourses,

                // Performance indicators
                CourseUtilizationRate = courseCount > 0 ? Math.Round((double)activeCourseCount / courseCount * 100, 2) : 0,
                StudentEngagementRate = studentCount > 0 && totalEnrollments > 0 ? Math.Round((double)totalEnrollments / studentCount * 100, 2) : 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect application metrics");
            return new
            {
                Error = "Failed to collect application metrics",
                Timestamp = DateTime.UtcNow
            };
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stopping Metrics Collection Service...");

        // Perform final metrics collection
        await CollectMetrics();

        await base.StopAsync(stoppingToken);
    }
}
