using System.Security.Cryptography;
using System.Text;
using ERP.Domain.Common.Modules;

namespace ERP.Application.Common.Licensing;

/// <summary>
/// Interface for license integrity verification.
/// </summary>
public interface ILicenseIntegrityService
{
    /// <summary>
    /// Generates a hash for license data to detect tampering.
    /// </summary>
    string GenerateLicenseHash(OrganizationLicense license);

    /// <summary>
    /// Verifies that license data has not been tampered with.
    /// </summary>
    bool VerifyLicenseIntegrity(OrganizationLicense license, string storedHash);

    /// <summary>
    /// Generates a unique signature for the license.
    /// </summary>
    string GenerateLicenseSignature(Guid organizationId, string tier, DateTime endDate);

    /// <summary>
    /// Verifies the license signature.
    /// </summary>
    bool VerifyLicenseSignature(Guid organizationId, string tier, DateTime endDate, string signature);
}

/// <summary>
/// Service to verify license integrity and detect tampering.
/// </summary>
public class LicenseIntegrityService : ILicenseIntegrityService
{
    // In production, this should come from secure configuration
    private const string SecurityKey = "NEXTERP-LICENSE-INTEGRITY-KEY-V1";

    /// <inheritdoc />
    public string GenerateLicenseHash(OrganizationLicense license)
    {
        var dataToHash = $"{license.OrganizationId}|{license.LicenseTierId}|{license.StartDate:O}|{license.EndDate:O}|{license.MaxUsers}|{SecurityKey}";
        return ComputeSha256Hash(dataToHash);
    }

    /// <inheritdoc />
    public bool VerifyLicenseIntegrity(OrganizationLicense license, string storedHash)
    {
        if (string.IsNullOrEmpty(storedHash))
            return false;

        var currentHash = GenerateLicenseHash(license);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(currentHash),
            Encoding.UTF8.GetBytes(storedHash));
    }

    /// <inheritdoc />
    public string GenerateLicenseSignature(Guid organizationId, string tier, DateTime endDate)
    {
        var dataToSign = $"{organizationId}|{tier}|{endDate:O}|{SecurityKey}";
        return ComputeHmacSha256(dataToSign);
    }

    /// <inheritdoc />
    public bool VerifyLicenseSignature(Guid organizationId, string tier, DateTime endDate, string signature)
    {
        if (string.IsNullOrEmpty(signature))
            return false;

        var expectedSignature = GenerateLicenseSignature(organizationId, tier, endDate);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expectedSignature),
            Encoding.UTF8.GetBytes(signature));
    }

    private static string ComputeSha256Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string ComputeHmacSha256(string input)
    {
        var keyBytes = Encoding.UTF8.GetBytes(SecurityKey);
        var dataBytes = Encoding.UTF8.GetBytes(input);
        var hmac = HMACSHA256.HashData(keyBytes, dataBytes);
        return Convert.ToHexString(hmac).ToLowerInvariant();
    }
}

/// <summary>
/// Domain entity for storing license integrity data.
/// </summary>
public class LicenseIntegrityRecord
{
    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public string LicenseHash { get; private set; } = string.Empty;
    public string Signature { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public bool IsValid { get; private set; }

    // Required for EF Core
    private LicenseIntegrityRecord() { }

    public LicenseIntegrityRecord(Guid organizationId, string licenseHash, string signature)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        LicenseHash = licenseHash;
        Signature = signature;
        CreatedAt = DateTime.UtcNow;
        IsValid = true;
    }

    public void MarkVerified()
    {
        VerifiedAt = DateTime.UtcNow;
    }

    public void MarkInvalid(string reason)
    {
        IsValid = false;
        // Could add a reason field for audit purposes
    }
}
