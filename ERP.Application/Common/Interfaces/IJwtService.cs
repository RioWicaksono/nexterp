using ERP.Domain.Base;

namespace ERP.Application.Common.Interfaces;

/// <summary>
/// JWT token generation and validation service with blacklist support
/// </summary>
public interface IJwtService
{
    (string token, DateTime expiresAt) GenerateAccessToken(User user, IEnumerable<string> permissions);
    string GenerateRefreshToken();
    Task<bool> ValidateTokenAsync(string token);
    Task<Guid?> GetUserIdFromTokenAsync(string token);

    /// <summary>
    /// Blacklists a token (for logout/revocation)
    /// </summary>
    Task BlacklistTokenAsync(string token, TimeSpan expiry);

    /// <summary>
    /// Checks if a token is blacklisted
    /// </summary>
    Task<bool> IsTokenBlacklistedAsync(string token);

    /// <summary>
    /// Extracts the JTI (JWT ID) from a token for blacklisting
    /// </summary>
    string? GetTokenId(string token);
}
