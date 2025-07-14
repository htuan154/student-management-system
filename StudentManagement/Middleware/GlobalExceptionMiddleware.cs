using System.Net;
using System.Text.Json;

namespace StudentManagementSystem.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("Processing request: {Method} {Path}", context.Request.Method, context.Request.Path);

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred. TraceId: {TraceId}", context.TraceIdentifier);
                await HandleExceptionAsync(context, ex);
            }
        }
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = exception switch
            {
                InvalidOperationException => new ErrorResponse
                {
                    Message = exception.Message,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    TraceId = context.TraceIdentifier
                },
                UnauthorizedAccessException => new ErrorResponse
                {
                    Message = "Unauthorized access",
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    TraceId = context.TraceIdentifier
                },
                KeyNotFoundException => new ErrorResponse
                {
                    Message = "Resource not found",
                    StatusCode = (int)HttpStatusCode.NotFound,
                    TraceId = context.TraceIdentifier
                },
                ArgumentException => new ErrorResponse
                {
                    Message = exception.Message,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    TraceId = context.TraceIdentifier
                },
                _ => new ErrorResponse
                {
                    Message = "An error occurred while processing your request",
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    TraceId = context.TraceIdentifier
                }
            };

            context.Response.StatusCode = response.StatusCode;

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string TraceId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
