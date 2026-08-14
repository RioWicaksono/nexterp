using System.Threading.RateLimiting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using ERP.Application.Common.Interfaces;

namespace ERP.Infrastructure.Services;

/// <summary>
/// Redis-backed sliding window rate limiter.
/// Uses Redis sorted sets for distributed rate limiting across multiple instances.
/// </summary>
public class RedisRateLimitService : IRateLimitService
{
	private readonly IConnectionMultiplexer _redis;
	private readonly ILogger<RedisRateLimitService> _logger;
	private readonly string _instanceName;

	public RedisRateLimitService(
		IConnectionMultiplexer redis,
		ILogger<RedisRateLimitService> logger,
		string instanceName = "nexterp")
	{
		_redis = redis;
		_logger = logger;
		_instanceName = instanceName;
	}

	/// <inheritdoc />
	public async Task<RateLimitResult> CheckRateLimitAsync(
		string key,
		int limit,
		int windowSeconds = 60,
		CancellationToken cancellationToken = default)
	{
		var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		var windowStart = now - windowSeconds;
		var fullKey = $"{_instanceName}:ratelimit:{key}";

		try
		{
			var db = _redis.GetDatabase();

			// Use Redis sorted set with timestamp as score
			// Remove expired entries
			// Count remaining
			// Add new entry if allowed
			// Return result

			// Lua script for atomic rate limiting
			var script = @"
				redis.call('ZREMRANGEBYSCORE', KEYS[1], '-inf', ARGV[1])
				local count = redis.call('ZCARD', KEYS[1])
				if count < tonumber(ARGV[3]) then
					redis.call('ZADD', KEYS[1], ARGV[2], ARGV[2] .. ':' .. count)
					redis.call('EXPIRE', KEYS[1], ARGV[4])
				end
				return {count, ARGV[3], ARGV[1] + ARGV[4]}
			";

			var result = await db.ScriptEvaluateAsync(
				script,
				new RedisKey[] { fullKey },
				new RedisValue[] { windowStart, now, limit, windowSeconds });

			var values = (RedisResult[])result!;
			var currentCount = (int)values[0];
			var allowed = currentCount < limit;
			var remaining = Math.Max(0, limit - currentCount - (allowed ? 1 : 0));
			var resetAt = DateTimeOffset.FromUnixTimeSeconds((long)values[2]).UtcDateTime;

			return new RateLimitResult(allowed, remaining, resetAt, limit);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Redis rate limit check failed for key {Key}. Allowing request.", key);

			// Fail open - allow request if Redis is unavailable
			return new RateLimitResult(true, limit, DateTime.UtcNow.AddSeconds(windowSeconds), limit);
		}
	}
}
