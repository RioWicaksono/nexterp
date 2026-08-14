using ERP.Domain.Common.Modules;

namespace ERP.Application.Common.Licensing;

/// <summary>
/// License management service interface.
/// </summary>
public interface ILicenseService
{
    /// <summary>
    /// Get license for organization.
    /// </summary>
    Task<OrganizationLicense?> GetLicenseAsync(Guid organizationId);

    /// <summary>
    /// Create new license for organization.
    /// </summary>
    Task<OrganizationLicense> CreateLicenseAsync(CreateLicenseRequest request);

    /// <summary>
    /// Update license tier.
    /// </summary>
    Task<OrganizationLicense> UpdateLicenseTierAsync(Guid organizationId, string tier);

    /// <summary>
    /// Extend license expiration.
    /// </summary>
    Task<OrganizationLicense> ExtendLicenseAsync(Guid organizationId, DateTime newExpiry);

    /// <summary>
    /// Revoke license (expire immediately).
    /// </summary>
    Task RevokeLicenseAsync(Guid organizationId);

    /// <summary>
    /// Check if license is valid.
    /// </summary>
    Task<bool> IsLicenseValidAsync(Guid organizationId);

    /// <summary>
    /// Get all licenses (admin only).
    /// </summary>
    Task<List<OrganizationLicense>> GetAllLicensesAsync(bool? activeOnly = null);

    /// <summary>
    /// Get license statistics.
    /// </summary>
    Task<LicenseStatistics> GetStatisticsAsync();

    /// <summary>
    /// Check if organization has access to module.
    /// </summary>
    Task<bool> HasModuleAccessAsync(Guid organizationId, string moduleCode);

    /// <summary>
    /// Enable module for organization.
    /// </summary>
    Task EnableModuleAsync(Guid organizationId, string moduleCode);

    /// <summary>
    /// Disable module for organization.
    /// </summary>
    Task DisableModuleAsync(Guid organizationId, string moduleCode);

    /// <summary>
    /// Get enabled modules for organization.
    /// </summary>
    Task<List<string>> GetEnabledModulesAsync(Guid organizationId);

    /// <summary>
    /// Sync modules based on license tier.
    /// </summary>
    Task SyncModulesFromTierAsync(Guid organizationId);
}

/// <summary>
/// Create license request.
/// </summary>
public record CreateLicenseRequest(
    Guid OrganizationId,
    string Tier,
    int DurationDays = 365,
    string? Notes = null);

/// <summary>
/// Update license request.
/// </summary>
public record UpdateLicenseRequest(
    Guid OrganizationId,
    string Tier,
    DateTime? ExpiresAt = null,
    bool IsActive = true);

/// <summary>
/// License statistics.
/// </summary>
public record LicenseStatistics(
    int TotalOrganizations,
    int ActiveLicenses,
    int ExpiredLicenses,
    int ExpiringIn7Days,
    int ExpiringIn30Days,
    Dictionary<string, int> TierDistribution);

/// <summary>
/// Module access service interface (combined with license service).
/// </summary>
// Note: Using ILicenseService for module access methods as they share implementation
