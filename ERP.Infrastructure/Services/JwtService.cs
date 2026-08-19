using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using ERP.Application.Common.Configuration;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Base;

namespace ERP.Infrastructure.Services;

/// <summary>
/// JWT token service implementation with Redis-backed token blacklist
/// </summary>
public class JwtService : IJwtService
{
    private readonly JwtSettings _settings;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<JwtService> _logger;
    private const string BlacklistKeyPrefix = "jwt:blacklist:";

    public JwtService(JwtSettings settings, IConnectionMultiplexer redis, ILogger<JwtService> logger)
    {
        _settings = settings;
        _redis = redis;
        _logger = logger;
    }

    public (string token, DateTime expiresAt) GenerateAccessToken(User user, IEnumerable<string> permissions)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("uid", user.Id.ToString()),
            new("unm", user.Username),
            new("org", user.OrganizationId.ToString()),
            new("sadm", user.IsSuperAdmin.ToString().ToLower()),
            new("per", string.Join(",", permissions)),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Add role claims for authorization
        if (user.IsSuperAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "SuperAdmin"));
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_settings.SecretKey);

        try
        {
            // Check if token is blacklisted
            if (await IsTokenBlacklistedAsync(token))
            {
                _logger.LogDebug("Token validation failed: token is blacklisted");
                return false;
            }

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _settings.Issuer,
                ValidateAudience = true,
                ValidAudience = _settings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return true;
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogDebug("Token validation failed: token expired");
            return true; // Allow expired tokens (they'll be rejected by framework)
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Token validation failed");
            return false;
        }
    }

    public Task<Guid?> GetUserIdFromTokenAsync(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "uid");
            if (Guid.TryParse(userIdClaim?.Value, out var userId))
                return Task.FromResult<Guid?>(userId);
            return Task.FromResult<Guid?>(null);
        }
        catch
        {
            return Task.FromResult<Guid?>(null);
        }
    }

    public string? GetTokenId(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        }
        catch
        {
            return null;
        }
    }

    public async Task BlacklistTokenAsync(string token, TimeSpan expiry)
    {
        try
        {
            var db = _redis.GetDatabase();
            var tokenId = GetTokenId(token);
            if (string.IsNullOrEmpty(tokenId))
            {
                _logger.LogWarning("Cannot blacklist token: JTI not found");
                return;
            }

            var key = $"{BlacklistKeyPrefix}{tokenId}";
            await db.StringSetAsync(key, "1", expiry);

            _logger.LogInformation("Token blacklisted: {TokenId}, expires in {Expiry}", tokenId, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to blacklist token");
        }
    }

    public async Task<bool> IsTokenBlacklistedAsync(string token)
    {
        try
        {
            var db = _redis.GetDatabase();
            var tokenId = GetTokenId(token);
            if (string.IsNullOrEmpty(tokenId))
                return false;

            var key = $"{BlacklistKeyPrefix}{tokenId}";
            return await db.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check token blacklist");
            return false; // Fail open for availability
        }
    }
}
