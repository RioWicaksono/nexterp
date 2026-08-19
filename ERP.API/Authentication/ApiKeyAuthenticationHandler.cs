using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace ERP.API.Authentication;

/// <summary>
/// API Key authentication handler for external integrations.
/// Clients authenticate using X-Api-Key header.
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private const string ApiKeyHeaderName = "X-Api-Key";
    private readonly ILogger<ApiKeyAuthenticationHandler> _logger;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder)
        : base(options, loggerFactory, encoder)
    {
        _logger = loggerFactory.CreateLogger<ApiKeyAuthenticationHandler>();
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if API Key header exists
        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var providedApiKey = apiKeyHeaderValues.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(providedApiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("API Key is empty"));
        }

        // Validate API Key
        if (!ValidateApiKey(providedApiKey, out var clientId, out var permissions))
        {
            _logger.LogWarning("Invalid API Key attempt from IP: {IpAddress}", GetClientIp());
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
        }

        // Create claims principal
        var claims = new List<System.Security.Claims.Claim>
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, clientId),
            new("api_client", "true"),
            new("auth_type", "api_key")
        };

        // Add permissions
        foreach (var permission in permissions)
        {
            claims.Add(new System.Security.Claims.Claim("permission", permission));
        }

        var identity = new System.Security.Claims.ClaimsIdentity(claims, Scheme.Name);
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        _logger.LogInformation(
            "API Key authentication successful for client: {ClientId} from IP: {IpAddress}",
            clientId, GetClientIp());

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private bool ValidateApiKey(string providedKey, out string clientId, out List<string> permissions)
    {
        clientId = string.Empty;
        permissions = new List<string>();

        // Hash the provided key for comparison
        var providedKeyHash = ComputeSha256Hash(providedKey);

        // Check against configured API keys
        foreach (var keyConfig in Options.ApiKeys)
        {
            if (keyConfig.KeyHash == providedKeyHash)
            {
                clientId = keyConfig.ClientId;
                permissions = keyConfig.Permissions;
                return true;
            }
        }

        return false;
    }

    private static string ComputeSha256Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }

    private string GetClientIp()
    {
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
            return forwardedFor.Split(',')[0].Trim();

        return Context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

/// <summary>
/// Options for API Key authentication
/// </summary>
public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public List<ApiKeyConfig> ApiKeys { get; set; } = new();
}

/// <summary>
/// Configuration for a single API key
/// </summary>
public class ApiKeyConfig
{
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// The actual API key (will be stored as hash)
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// SHA-256 hash of the key (computed automatically)
    /// </summary>
    public string KeyHash { get; set; } = string.Empty;

    /// <summary>
    /// Permissions granted to this API key
    /// </summary>
    public List<string> Permissions { get; set; } = new();

    /// <summary>
    /// Whether this API key is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional expiration date
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Optional description/purpose
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Extension methods for API Key configuration
/// </summary>
public static class ApiKeyAuthenticationExtensions
{
    public static AuthenticationBuilder AddApiKeyAuthentication(
        this AuthenticationBuilder builder,
        Action<ApiKeyAuthenticationOptions> configureOptions)
    {
        return builder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
            "ApiKey",
            configureOptions);
    }

    /// <summary>
    /// Adds an API key to the configuration
    /// </summary>
    public static ApiKeyConfig AddApiKey(
        this List<ApiKeyConfig> apiKeys,
        string clientId,
        string key,
        params string[] permissions)
    {
        var config = new ApiKeyConfig
        {
            ClientId = clientId,
            Key = key,
            KeyHash = ComputeSha256Hash(key),
            Permissions = permissions.ToList()
        };

        apiKeys.Add(config);
        return config;
    }

    private static string ComputeSha256Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}
