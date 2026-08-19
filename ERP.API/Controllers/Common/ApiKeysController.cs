using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using ERP.API.Controllers.Base;

namespace ERP.API.Controllers.Common;

/// <summary>
/// Controller for API Key management (SuperAdmin only)
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/api-keys")]
[Authorize(Policy = "RequireSuperAdmin")]
public class ApiKeysController : BaseApiController
{
    /// <summary>
    /// Generate a new API key
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiKeyResponse), StatusCodes.Status201Created)]
    public IActionResult CreateApiKey([FromBody] CreateApiKeyRequest request)
    {
        // Generate new API key
        var apiKey = GenerateSecureApiKey();
        var keyHash = ComputeSha256Hash(apiKey);

        // In production, this would save to database
        // For now, return the key (it should be shown only once)
        return StatusCode(201, new ApiKeyResponse
        {
            ClientId = request.ClientId,
            ApiKey = apiKey,
            KeyHash = keyHash,
            Permissions = request.Permissions,
            CreatedAt = DateTime.UtcNow,
            Message = "Save this API key securely. It will not be shown again."
        });
    }

    /// <summary>
    /// Validate an API key (for testing purposes)
    /// </summary>
    [HttpPost("validate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiKeyValidationResponse), StatusCodes.Status200OK)]
    public IActionResult ValidateApiKey([FromBody] ValidateApiKeyRequest request)
    {
        var keyHash = ComputeSha256Hash(request.ApiKey);

        return Ok(new ApiKeyValidationResponse
        {
            IsValid = true, // In production, check against stored hashes
            ClientId = "demo-client",
            Permissions = new[] { "reports.read", "analytics.dashboard.read" }
        });
    }

    private static string GenerateSecureApiKey()
    {
        var bytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return $"nexterp_{Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "")}";
    }

    private static string ComputeSha256Hash(string input)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}

public class CreateApiKeyRequest
{
    public string ClientId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public DateTime? ExpiresAt { get; set; }
}

public class ApiKeyResponse
{
    public string ClientId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string KeyHash { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ValidateApiKeyRequest
{
    public string ApiKey { get; set; } = string.Empty;
}

public class ApiKeyValidationResponse
{
    public bool IsValid { get; set; }
    public string? ClientId { get; set; }
    public string[]? Permissions { get; set; }
}
