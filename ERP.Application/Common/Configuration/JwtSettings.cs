namespace ERP.Application.Common.Configuration;

/// <summary>
/// JWT configuration settings - shared between API and Application layers
/// </summary>
public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    // Access token: 15 minutes (production standard - reduced from 60 for security)
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    // Refresh token: 7 days with rotation enabled
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
