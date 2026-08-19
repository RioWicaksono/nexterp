using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ERP.API.Middleware;

/// <summary>
/// Middleware for structured API request/response logging.
/// Logs request details, response status, and timing for audit purposes.
/// </summary>
public class ApiAuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiAuditLoggingMiddleware> _logger;

    // Paths to exclude from audit logging
    private static readonly string[] ExcludedPaths = new[]
    {
        "/health",
        "/metrics",
        "/swagger",
        "/favicon.ico"
    };

    public ApiAuditLoggingMiddleware(RequestDelegate next, ILogger<ApiAuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Skip excluded paths
        if (ExcludedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
        var requestId = Guid.NewGuid().ToString("N")[..8];

        // Capture request details
        var requestBody = await CaptureRequestBodyAsync(context);
        var userId = context.User?.FindFirst("uid")?.Value ?? "anonymous";
        var ipAddress = GetClientIp(context);
        var userAgent = context.Request.Headers.UserAgent.ToString();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            // Log request
            _logger.LogInformation(
                "API Request: {RequestId} | {Method} {Path} | " +
                "User: {UserId} | IP: {IpAddress} | " +
                "Status: {StatusCode} | Duration: {Duration}ms | " +
                "CorrelationId: {CorrelationId}",
                requestId,
                context.Request.Method,
                path,
                userId,
                ipAddress,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId);

            // Log warning for slow requests (>1s)
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning(
                    "Slow Request: {RequestId} | {Method} {Path} took {Duration}ms",
                    requestId,
                    context.Request.Method,
                    path,
                    stopwatch.ElapsedMilliseconds);
            }

            // Log error responses
            if (context.Response.StatusCode >= 400)
            {
                _logger.LogWarning(
                    "API Response Error: {RequestId} | {Method} {Path} | " +
                    "Status: {StatusCode} | User: {UserId}",
                    requestId,
                    context.Request.Method,
                    path,
                    context.Response.StatusCode,
                    userId);
            }
        }
    }

    private static async Task<string?> CaptureRequestBodyAsync(HttpContext context)
    {
        if (context.Request.ContentLength == null || context.Request.ContentLength == 0)
            return null;

        if (context.Request.ContentType == null)
            return null;

        // Only capture JSON bodies
        if (!context.Request.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            return null;

        // Enable buffering so the body can be read again
        context.Request.EnableBuffering();

        try
        {
            using var reader = new StreamReader(
                context.Request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();

            // Reset position for downstream handlers
            context.Request.Body.Position = 0;

            // Truncate long bodies
            if (body.Length > 4096)
                body = body[..4096] + "...[truncated]";

            return body;
        }
        catch
        {
            return "[Unable to read body]";
        }
    }

    private static string GetClientIp(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
            return forwardedFor.Split(',')[0].Trim();

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

/// <summary>
/// Extension method for registering the audit logging middleware
/// </summary>
public static class ApiAuditLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseApiAuditLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ApiAuditLoggingMiddleware>();
    }
}
