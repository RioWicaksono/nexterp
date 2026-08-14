using System.Net;
using System.Text.Json;
using ERP.Application.Common.Interfaces;

namespace ERP.API.Middleware;

public class RateLimitingMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<RateLimitingMiddleware> _logger;
	private readonly IRateLimitService _rateLimitService;

	private const int AnonymousLimit = 100;  // requests per minute
	private const int AuthenticatedLimit = 1000; // requests per minute
	private const int WindowSeconds = 60;

	public RateLimitingMiddleware(
		RequestDelegate next,
		ILogger<RateLimitingMiddleware> logger,
		IRateLimitService rateLimitService)
	{
		_next = next;
		_logger = logger;
		_rateLimitService = rateLimitService;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		var clientIp = GetClientIp(context);
		var userId = context.User?.Identity?.Name ?? "anonymous";
		var key = context.User?.Identity?.IsAuthenticated == true ? $"user:{userId}" : $"ip:{clientIp}";

		var limit = context.User?.Identity?.IsAuthenticated == true ? AuthenticatedLimit : AnonymousLimit;

		var result = await _rateLimitService.CheckRateLimitAsync(key, limit, WindowSeconds);

		// Add rate limit headers
		context.Response.OnStarting(() =>
		{
			context.Response.Headers["X-RateLimit-Limit"] = result.Limit.ToString();
			context.Response.Headers["X-RateLimit-Remaining"] = result.Remaining.ToString();
			context.Response.Headers["X-RateLimit-Reset"] = new DateTimeOffset(result.ResetAt).ToUnixTimeSeconds().ToString();
			return Task.CompletedTask;
		});

		if (!result.IsAllowed)
		{
			_logger.LogWarning("Rate limit exceeded for {Key} from IP {ClientIp}", key, clientIp);

			context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
			context.Response.ContentType = "application/json";
			context.Response.Headers.Append("Retry-After", WindowSeconds.ToString());

			var response = new
			{
				success = false,
				error = "Rate limit exceeded. Please try again later.",
				retryAfter = WindowSeconds,
				limit = result.Limit,
				remaining = 0
			};

			await context.Response.WriteAsJsonAsync(response);
			return;
		}

		await _next(context);
	}

	private static string GetClientIp(HttpContext context)
	{
		var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
		if (!string.IsNullOrEmpty(forwardedFor))
			return forwardedFor.Split(',')[0].Trim();

		return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
	}
}

public static class RateLimitingMiddlewareExtensions
{
	public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
	{
		return builder.UseMiddleware<RateLimitingMiddleware>();
	}
}
