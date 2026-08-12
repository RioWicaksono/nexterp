using ERP.Domain.Base;

namespace ERP.Application.Common.Interfaces;

/// <summary>
/// JWT token generation and validation service
/// </summary>
public interface IJwtService
{
    (string token, DateTime expiresAt) GenerateAccessToken(User user, IEnumerable<string> permissions);
    string GenerateRefreshToken();
    Task<bool> ValidateTokenAsync(string token);
    Task<Guid?> GetUserIdFromTokenAsync(string token);
}
