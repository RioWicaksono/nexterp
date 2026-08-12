using System.Net;
using System.Text.Json;
using Serilog;

namespace ERP.API.Middleware;

/// <summary>
/// Global exception handler middleware that catches all unhandled exceptions
/// and returns consistent JSON error responses while logging details securely.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = GetOrGenerateCorrelationId(context);

        // Determine status code and response based on exception type
        var (statusCode, errorResponse) = MapExceptionToResponse(exception, correlationId);

        // Log full exception details internally (correlation ID included for tracing)
        LogExceptionWithContext(exception, correlationId, context);

        // Return sanitized error response to client
        await WriteErrorResponseAsync(context, statusCode, errorResponse);
    }

    private static string GetOrGenerateCorrelationId(HttpContext context)
    {
        const string correlationIdHeader = "X-Correlation-ID";
        const string correlationIdKey = "CorrelationId";

        if (context.Request.Headers.TryGetValue(correlationIdHeader, out var existingId)
            && !string.IsNullOrWhiteSpace(existingId))
        {
            return existingId.ToString();
        }

        var newCorrelationId = Guid.NewGuid().ToString();
        context.Items[correlationIdKey] = newCorrelationId;
        context.Response.Headers[correlationIdHeader] = newCorrelationId;

        return newCorrelationId;
    }

    private static (HttpStatusCode, ErrorResponse) MapExceptionToResponse(Exception exception, string correlationId)
    {
        return exception switch
        {
            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse(
                    Success: false,
                    Error: argEx.Message,
                    CorrelationId: correlationId
                )
            ),

            UnauthorizedAccessException unauthorizedEx => (
                HttpStatusCode.Unauthorized,
                new ErrorResponse(
                    Success: false,
                    Error: "Access denied. Please ensure you have proper authentication.",
                    CorrelationId: correlationId
                )
            ),

            KeyNotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                new ErrorResponse(
                    Success: false,
                    Error: notFoundEx.Message,
                    CorrelationId: correlationId
                )
            ),

            InvalidOperationException invalidOpEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse(
                    Success: false,
                    Error: invalidOpEx.Message,
                    CorrelationId: correlationId
                )
            ),

            // FluentValidation validation failures
            FluentValidation.ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse(
                    Success: false,
                    Error: string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage)),
                    CorrelationId: correlationId
                )
            ),

            // Default: Internal server error - never expose details
            _ => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse(
                    Success: false,
                    Error: "An unexpected error occurred. Please try again later or contact support.",
                    CorrelationId: correlationId
                )
            )
        };
    }

    private void LogExceptionWithContext(Exception exception, string correlationId, HttpContext context)
    {
        var requestPath = context.Request.Path.Value;
        var requestMethod = context.Request.Method;
        var userId = context.User?.Identity?.Name ?? "anonymous";
        var userAgent = context.Request.Headers.UserAgent.ToString();

        // Structured logging with Serilog - includes correlation ID for tracing
        _logger.LogError(
            exception,
            "Unhandled exception occurred. " +
            "CorrelationId: {CorrelationId}, " +
            "RequestPath: {RequestPath}, " +
            "RequestMethod: {RequestMethod}, " +
            "UserId: {UserId}, " +
            "UserAgent: {UserAgent}, " +
            "ExceptionType: {ExceptionType}, " +
            "ExceptionMessage: {ExceptionMessage}",
            correlationId,
            requestPath,
            requestMethod,
            userId,
            userAgent,
            exception.GetType().Name,
            exception.Message
        );

        // Log stack trace at Debug level (not exposed to external monitoring systems)
        Log.ForContext("StackTrace", exception.StackTrace, destructureObjects: true)
           .ForContext("InnerException", exception.InnerException?.Message)
           .Debug("Exception stack trace for CorrelationId: {CorrelationId}", correlationId);
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, HttpStatusCode statusCode, ErrorResponse errorResponse)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(errorResponse, jsonOptions);
        await context.Response.WriteAsync(json);
    }

    /// <summary>
    /// Standardized error response structure for consistent API error format
    /// </summary>
    private record ErrorResponse(bool Success, string Error, string CorrelationId);
}

/// <summary>
/// Extension methods for registering the GlobalExceptionHandlerMiddleware
/// </summary>
public static class GlobalExceptionHandlerMiddlewareExtensions
{
    /// <summary>
    /// Adds the global exception handler middleware to the application pipeline.
    /// Must be registered early in the pipeline to catch all exceptions.
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
