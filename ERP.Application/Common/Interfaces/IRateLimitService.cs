namespace ERP.Application.Common.Interfaces;

/// <summary>
/// Interface for rate limiting service.
/// Implementations can use in-memory or distributed storage (Redis).
/// </summary>
public interface IRateLimitService
{
	/// <summary>
	/// Attempts to consume a rate limit token for the given key.
	/// Returns true if the request is allowed, false if rate limited.
	/// </summary>
	/// <param name="key">Unique identifier for rate limiting (e.g., "user:123" or "ip:192.168.1.1")</param>
	/// <param name="limit">Maximum requests allowed in the window</param>
	/// <param name="windowSeconds">Window duration in seconds</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Tuple of (isAllowed, remaining, resetAt)</returns>
	Task<RateLimitResult> CheckRateLimitAsync(string key, int limit, int windowSeconds = 60, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a rate limit check.
/// </summary>
public record RateLimitResult(bool IsAllowed, int Remaining, DateTime ResetAt, int Limit);
