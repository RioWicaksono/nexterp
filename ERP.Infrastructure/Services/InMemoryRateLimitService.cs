using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using ERP.Application.Common.Interfaces;

namespace ERP.Infrastructure.Services;

/// <summary>
/// In-memory sliding window rate limiter.
/// Fallback when Redis is unavailable or for single-instance deployments.
/// </summary>
public class InMemoryRateLimitService : IRateLimitService
{
	private readonly ILogger<InMemoryRateLimitService> _logger;
	private readonly ConcurrentDictionary<string, RateLimitBucket> _buckets = new();

	public InMemoryRateLimitService(ILogger<InMemoryRateLimitService> logger)
	{
		_logger = logger;
	}

	/// <inheritdoc />
	public Task<RateLimitResult> CheckRateLimitAsync(
		string key,
		int limit,
		int windowSeconds = 60,
		CancellationToken cancellationToken = default)
	{
		var now = DateTimeOffset.UtcNow;
		var windowStart = now.AddSeconds(-windowSeconds);
		var resetAt = now.AddSeconds(windowSeconds);

		var bucket = _buckets.GetOrAdd(key, _ => new RateLimitBucket());

		lock (bucket)
		{
			// Remove expired entries
			bucket.Requests.RemoveAll(t => t < windowStart);

			// Check limit
			if (bucket.Requests.Count >= limit)
			{
				_logger.LogDebug("Rate limit exceeded for {Key}: {Count}/{Limit}", key, bucket.Requests.Count, limit);
				return Task.FromResult(new RateLimitResult(false, 0, resetAt.DateTime, limit));
			}

			// Add new request
			bucket.Requests.Add(now);
			var remaining = limit - bucket.Requests.Count - 1;

			return Task.FromResult(new RateLimitResult(true, remaining, resetAt.DateTime, limit));
		}
	}

	private class RateLimitBucket
	{
		public List<DateTimeOffset> Requests { get; } = new();
	}
}
