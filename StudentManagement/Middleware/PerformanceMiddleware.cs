// // Middleware/PerformanceMiddleware.cs
// using System.Diagnostics;

// namespace StudentManagementSystem.Middleware;

// public class PerformanceMiddleware
// {
//     private readonly RequestDelegate _next;
//     private readonly ILogger<PerformanceMiddleware> _logger;
//     private readonly ApplicationMetrics _metrics;

//     public PerformanceMiddleware(
//         RequestDelegate next,
//         ILogger<PerformanceMiddleware> logger,
//         ApplicationMetrics metrics)
//     {
//         _next = next;
//         _logger = logger;
//         _metrics = metrics;
//     }

//     public async Task InvokeAsync(HttpContext context)
//     {
//         var stopwatch = Stopwatch.StartNew();
//         var requestPath = context.Request.Path.Value ?? "";
//         var method = context.Request.Method;

//         try
//         {
//             await _next(context);
//         }
//         finally
//         {
//             stopwatch.Stop();
//             var elapsedMs = stopwatch.ElapsedMilliseconds;
//             var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;

//             // Record metrics
//             _metrics.RequestDuration.Record(elapsedSeconds, new KeyValuePair<string, object?>("method", method),
//                 new KeyValuePair<string, object?>("path", requestPath));

//             // Log slow requests
//             if (elapsedMs > 1000) // Requests slower than 1 second
//             {
//                 _logger.LogWarning("Slow request: {Method} {Path} took {ElapsedMs}ms",
//                     method, requestPath, elapsedMs);
//             }
//             else if (elapsedMs > 5000) // Requests slower than 5 seconds
//             {
//                 _logger.LogError("Very slow request: {Method} {Path} took {ElapsedMs}ms",
//                     method, requestPath, elapsedMs);
//             }

//             // Add performance header
//             context.Response.Headers.Add("X-Response-Time", $"{elapsedMs}ms");
//         }
//     }
// }
