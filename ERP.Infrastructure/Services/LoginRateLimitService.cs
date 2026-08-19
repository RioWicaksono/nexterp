using System.Threading.RateLimiting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using ERP.Application.Common.Interfaces;

namespace ERP.Infrastructure.Services;

/// <summary>
/// Redis-backed login attempt rate limiter for brute force protection.
/// Tracks failed login attempts per IP and username with progressive lockout.
/// </summary>
public class LoginRateLimitService : ILoginRateLimitService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<LoginRateLimitService> _logger;
    private const string KeyPrefix = "login:ratelimit:";

    // Progressive lockout settings
    private const int MaxAttemptsPerWindow = 5;        // 5 attempts
    private const int WindowSeconds = 300;              // 5 minutes
    private const int LockoutSeconds = 900;            // 15 minutes lockout

    public LoginRateLimitService(
        IConnectionMultiplexer redis,
        ILogger<LoginRateLimitService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<LoginRateLimitResult> CheckLoginAttemptAsync(
        string ipAddress,
        string username,
        bool isFailedAttempt,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var windowStart = now - WindowSeconds;
        var ipKey = $"{KeyPrefix}ip:{ipAddress}";
        var userKey = $"{KeyPrefix}user:{username.ToLowerInvariant()}";

        try
        {
            var db = _redis.GetDatabase();

            // Check if currently locked out
            var ipLockKey = $"{ipKey}:lock";
            var ipLockValue = await db.StringGetAsync(ipLockKey);
            if (ipLockValue.HasValue)
            {
                var lockExpiry = await db.KeyTimeToLiveAsync(ipLockKey);
                var expirySeconds = (long)(lockExpiry ?? TimeSpan.Zero).TotalSeconds;
                return new LoginRateLimitResult(
                    IsLocked: true,
                    RemainingAttempts: 0,
                    LockoutExpiresAt: DateTimeOffset.FromUnixTimeSeconds(now + expirySeconds).UtcDateTime,
                    RetryAfterSeconds: (int)expirySeconds
                );
            }

            // Lua script for atomic increment and check
            var script = @"
                -- Remove expired entries
                redis.call('ZREMRANGEBYSCORE', KEYS[1], '-inf', ARGV[1])

                -- Count attempts in window
                local count = redis.call('ZCARD', KEYS[1])

                -- If failed attempt, add new entry
                if ARGV[4] == '1' then
                    redis.call('ZADD', KEYS[1], ARGV[2], ARGV[2] .. ':' .. count)
                    redis.call('EXPIRE', KEYS[1], ARGV[3])
                    count = count + 1
                end

                return count
            ";

            var countResult = await db.ScriptEvaluateAsync(
                script,
                new RedisKey[] { ipKey },
                new RedisValue[] { windowStart, now, WindowSeconds, isFailedAttempt ? "1" : "0" });

            var attemptCount = (int)countResult;
            var remainingAttempts = Math.Max(0, MaxAttemptsPerWindow - attemptCount);

            // Check if exceeded max attempts
            if (attemptCount >= MaxAttemptsPerWindow)
            {
                // Set lockout
                await db.StringSetAsync(ipLockKey, "locked", TimeSpan.FromSeconds(LockoutSeconds));

                _logger.LogWarning(
                    "Login brute force protection triggered for IP {IpAddress}, username {Username}. " +
                    "Locked for {LockoutSeconds} seconds after {AttemptCount} attempts",
                    ipAddress, username, LockoutSeconds, attemptCount);

                return new LoginRateLimitResult(
                    IsLocked: true,
                    RemainingAttempts: 0,
                    LockoutExpiresAt: DateTimeOffset.UtcNow.AddSeconds(LockoutSeconds).UtcDateTime,
                    RetryAfterSeconds: LockoutSeconds
                );
            }

            // If this was a failed attempt, also track for username
            if (isFailedAttempt)
            {
                await db.ScriptEvaluateAsync(
                    script,
                    new RedisKey[] { userKey },
                    new RedisValue[] { windowStart, now, WindowSeconds, "1" });
            }

            return new LoginRateLimitResult(
                IsLocked: false,
                RemainingAttempts: remainingAttempts,
                LockoutExpiresAt: null,
                RetryAfterSeconds: 0
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login rate limit check failed for IP {IpAddress}", ipAddress);
            // Fail open for availability
            return new LoginRateLimitResult(
                IsLocked: false,
                RemainingAttempts: MaxAttemptsPerWindow,
                LockoutExpiresAt: null,
                RetryAfterSeconds: 0
            );
        }
    }

    /// <inheritdoc />
    public async Task ClearLoginAttemptsAsync(string ipAddress, string username, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var ipKey = $"{KeyPrefix}ip:{ipAddress}";
            var userKey = $"{KeyPrefix}user:{username.ToLowerInvariant()}";
            var ipLockKey = $"{ipKey}:lock";

            await db.KeyDeleteAsync(new RedisKey[] { ipKey, userKey, ipLockKey });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear login attempts for IP {IpAddress}", ipAddress);
        }
    }
}

/// <summary>
/// Result of login rate limit check
/// </summary>
public record LoginRateLimitResult(
    bool IsLocked,
    int RemainingAttempts,
    DateTime? LockoutExpiresAt,
    int RetryAfterSeconds
);

/// <summary>
/// Interface for login-specific rate limiting service
/// </summary>
public interface ILoginRateLimitService
{
    /// <summary>
    /// Checks if login attempt is allowed
    /// </summary>
    Task<LoginRateLimitResult> CheckLoginAttemptAsync(
        string ipAddress,
        string username,
        bool isFailedAttempt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears login attempts on successful login
    /// </summary>
    Task ClearLoginAttemptsAsync(string ipAddress, string username, CancellationToken cancellationToken = default);
}
