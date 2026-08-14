using ERP.Domain.Common.Modules;

namespace ERP.Application.Common.Interfaces;

/// <summary>
/// Service to check module access and license validity for organizations.
/// </summary>
public interface IModuleAccessService
{
    /// <summary>
    /// Checks if an organization has access to a specific module.
    /// </summary>
    Task<bool> HasModuleAccessAsync(Guid organizationId, string moduleCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an organization's license is currently valid.
    /// </summary>
    Task<bool> IsOrganizationLicensedAsync(Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all activated modules for an organization.
    /// </summary>
    Task<IEnumerable<string>> GetActivatedModulesAsync(Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates a module for an organization.
    /// </summary>
    Task<Guid> ActivateModuleAsync(Guid organizationId, string moduleCode, string? activatedBy = null, DateTime? expiresAt = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a module for an organization.
    /// </summary>
    Task DeactivateModuleAsync(Guid organizationId, string moduleCode, string? deactivatedBy = null, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the license tier for an organization.
    /// </summary>
    Task<LicenseTier?> GetLicenseTierAsync(Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an organization has reached its maximum user limit.
    /// </summary>
    Task<bool> HasUserCapacityAsync(Guid organizationId, int additionalUsers = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a setting value for an organization.
    /// </summary>
    Task<T?> GetSettingAsync<T>(Guid organizationId, string settingKey, T? defaultValue = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a setting value for an organization.
    /// </summary>
    Task SetSettingAsync(Guid organizationId, string settingKey, string value, string category, string? description = null, bool isEncrypted = false, CancellationToken cancellationToken = default);
}
