using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;

namespace ERP.API.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, RateLimitInfo> _rateLimits = new();

    private const int AnonymousLimit = 100;  // requests per minute
    private const int AuthenticatedLimit = 1000; // requests per minute

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = GetClientIp(context);
        var userId = context.User?.Identity?.Name ?? "anonymous";
        var key = context.User?.Identity?.IsAuthenticated == true ? $"user:{userId}" : $"ip:{clientIp}";

        var limit = context.User?.Identity?.IsAuthenticated == true ? AuthenticatedLimit : AnonymousLimit;

        if (!TryConsume(key, limit))
        {
            _logger.LogWarning("Rate limit exceeded for {Key} from IP {ClientIp}", key, clientIp);

            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";
            context.Response.Headers.Append("Retry-After", "60");
            context.Response.Headers.Append("X-RateLimit-Limit", limit.ToString());
            context.Response.Headers.Append("X-RateLimit-Remaining", "0");

            var response = new
            {
                success = false,
                error = "Rate limit exceeded. Please try again later.",
                retryAfter = 60,
                limit = limit
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            return;
        }

        await _next(context);
    }

    private bool TryConsume(string key, int limit)
    {
        var now = DateTime.UtcNow;
        var windowStart = now.AddMinutes(-1);

        var rateLimit = _rateLimits.GetOrAdd(key, _ => new RateLimitInfo());

        lock (rateLimit)
        {
            rateLimit.Requests.RemoveAll(t => t < windowStart);

            if (rateLimit.Requests.Count >= limit)
                return false;

            rateLimit.Requests.Add(now);
            return true;
        }
    }

    private static string GetClientIp(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
            return forwardedFor.Split(',')[0].Trim();

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private class RateLimitInfo
    {
        public List<DateTime> Requests { get; } = new();
    }
}

public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}
