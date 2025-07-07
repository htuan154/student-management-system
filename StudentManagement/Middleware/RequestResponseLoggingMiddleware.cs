using System.Diagnostics;
using System.Text;

namespace StudentManagementSystem.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            // Log incoming request
            await LogRequestAsync(context);

            // Store original response body stream
            var originalBodyStream = context.Response.Body;

            try
            {
                // Create a new memory stream to capture response
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                // Call the next middleware
                await _next(context);

                // Log response
                await LogResponseAsync(context, responseBody, stopwatch.ElapsedMilliseconds);

                // Copy response back to original stream
                await responseBody.CopyToAsync(originalBodyStream);
            }
            finally
            {
                // Restore original response body stream
                context.Response.Body = originalBodyStream;
                stopwatch.Stop();
            }
        }

        private async Task LogRequestAsync(HttpContext context)
        {
            try
            {
                var request = context.Request;
                var requestBody = string.Empty;

                // Only log request body for POST/PUT requests and if content type is JSON
                if ((request.Method == "POST" || request.Method == "PUT") &&
                    request.ContentType?.Contains("application/json") == true)
                {
                    request.EnableBuffering();
                    var buffer = new byte[Convert.ToInt32(request.ContentLength)];
                    await request.Body.ReadAsync(buffer, 0, buffer.Length);
                    requestBody = Encoding.UTF8.GetString(buffer);
                    request.Body.Position = 0; // Reset position for next middleware
                }

                _logger.LogInformation(
                    "HTTP Request: {Method} {Path} {QueryString} | IP: {RemoteIpAddress} | TraceId: {TraceId} | Body: {RequestBody}",
                    request.Method,
                    request.Path,
                    request.QueryString,
                    context.Connection.RemoteIpAddress,
                    context.TraceIdentifier,
                    string.IsNullOrEmpty(requestBody) ? "N/A" : requestBody
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging request");
            }
        }

        private async Task LogResponseAsync(HttpContext context, MemoryStream responseBody, long elapsedMilliseconds)
        {
            try
            {
                var response = context.Response;
                responseBody.Seek(0, SeekOrigin.Begin);
                var responseBodyText = await new StreamReader(responseBody).ReadToEndAsync();
                responseBody.Seek(0, SeekOrigin.Begin);

                _logger.LogInformation(
                    "HTTP Response: {StatusCode} | TraceId: {TraceId} | Duration: {ElapsedMilliseconds}ms | Body: {ResponseBody}",
                    response.StatusCode,
                    context.TraceIdentifier,
                    elapsedMilliseconds,
                    string.IsNullOrEmpty(responseBodyText) ? "N/A" :
                    (responseBodyText.Length > 1000 ? responseBodyText.Substring(0, 1000) + "..." : responseBodyText)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging response");
            }
        }
    }
}
