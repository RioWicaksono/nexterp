using ERP.Application.Common.Behaviors;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Licensing;
using ERP.Application.Common.Modules;
using Microsoft.Extensions.Logging;

namespace ERP.Infrastructure.Services;

/// <summary>
/// Service to check license and module access for the current user.
/// Includes audit logging and tampering detection.
/// </summary>
public class LicenseCheckService : ILicenseCheckService
{
    private readonly ICurrentUserService _currentUser;
    private readonly ILicenseService _licenseService;
    private readonly ILicenseAuditService? _auditService;
    private readonly ILicenseIntegrityService? _integrityService;
    private readonly ILogger<LicenseCheckService> _logger;

    public LicenseCheckService(
        ICurrentUserService currentUser,
        ILicenseService licenseService,
        ILicenseAuditService? auditService = null,
        ILicenseIntegrityService? integrityService = null,
        ILogger<LicenseCheckService>? logger = null)
    {
        _currentUser = currentUser;
        _licenseService = licenseService;
        _auditService = auditService;
        _integrityService = integrityService;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<LicenseCheckService>.Instance;
    }

    public async Task<LicenseValidationResult> ValidateModuleAccessAsync(
        string moduleCode,
        string? featureName = null)
    {
        // SuperAdmin bypasses all checks
        if (_currentUser.IsSuperAdmin)
        {
            _logger.LogDebug("SuperAdmin bypass for module {Module}", moduleCode);
            return new LicenseValidationResult(true, null, moduleCode);
        }

        // Check organization ID
        var organizationId = _currentUser.OrganizationId;
        if (organizationId == null)
        {
            var error = "Organization not found in context";
            _logger.LogWarning("License validation failed: {Error}", error);
            await LogValidationAttempt(organizationId, moduleCode, false, error);
            return new LicenseValidationResult(false, error, moduleCode);
        }

        // Verify license integrity if service is available
        if (_integrityService != null)
        {
            var integrityValid = await VerifyLicenseIntegrityAsync(organizationId.Value);
            if (!integrityValid)
            {
                var error = "License integrity check failed - possible tampering detected";
                _logger.LogWarning(
                    "License tampering detected for organization {OrganizationId}, module {Module}",
                    organizationId, moduleCode);

                await _auditService?.LogTamperingDetectedAsync(
                    organizationId.Value,
                    $"Module access attempt for {moduleCode}: {error}");

                return new LicenseValidationResult(false, error, moduleCode);
            }
        }

        // Check license validity
        var isLicenseValid = await _licenseService.IsLicenseValidAsync(organizationId.Value);
        if (!isLicenseValid)
        {
            var error = "License is invalid or expired. Please contact administrator.";
            _logger.LogWarning(
                "License validation failed for org {OrganizationId}: {Error}",
                organizationId, error);
            await LogValidationAttempt(organizationId, moduleCode, false, error);
            return new LicenseValidationResult(false, error, moduleCode);
        }

        // Check module access
        var hasAccess = await _licenseService.HasModuleAccessAsync(organizationId.Value, moduleCode);
        if (!hasAccess)
        {
            var error = $"Module '{moduleCode}' is not enabled for your organization. Please contact administrator to enable this module.";
            _logger.LogWarning(
                "Module access denied for org {OrganizationId}: {Module} not enabled",
                organizationId, moduleCode);
            await LogValidationAttempt(organizationId, moduleCode, false, error);
            return new LicenseValidationResult(false, error, moduleCode);
        }

        // Check feature if specified
        if (!string.IsNullOrEmpty(featureName))
        {
            var config = ModuleConfigurationLoader.LoadModuleConfig(moduleCode);
            if (config.Features.TryGetValue(featureName, out var feature))
            {
                if (!feature.Enabled)
                {
                    var error = $"Feature '{featureName}' in module '{moduleCode}' is not enabled.";
                    await LogValidationAttempt(organizationId, moduleCode, false, error);
                    return new LicenseValidationResult(false, error, moduleCode);
                }
            }
        }

        // Log successful validation
        await LogValidationAttempt(organizationId, moduleCode, true);
        return new LicenseValidationResult(true, null, moduleCode);
    }

    public async Task<bool> IsLicenseValidAsync()
    {
        if (_currentUser.IsSuperAdmin)
            return true;

        var organizationId = _currentUser.OrganizationId;
        if (organizationId == null)
            return false;

        // Verify integrity if available
        if (_integrityService != null)
        {
            var integrityValid = await VerifyLicenseIntegrityAsync(organizationId.Value);
            if (!integrityValid)
            {
                _logger.LogWarning(
                    "License integrity check failed for org {OrganizationId}",
                    organizationId);
                return false;
            }
        }

        return await _licenseService.IsLicenseValidAsync(organizationId.Value);
    }

    private async Task<bool> VerifyLicenseIntegrityAsync(Guid organizationId)
    {
        try
        {
            // Get license from database
            var license = await _licenseService.GetLicenseAsync(organizationId);
            if (license == null)
                return false;

            // In a full implementation, you would:
            // 1. Retrieve stored hash from a separate integrity table
            // 2. Verify the hash matches current license state
            // 3. Check for any anomalies

            // For now, we verify the basic integrity
            return license.IsActive && !license.IsExpired;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during license integrity verification");
            return false;
        }
    }

    private async Task LogValidationAttempt(Guid? organizationId, string moduleCode, bool success, string? errorMessage = null)
    {
        if (_auditService == null)
            return;

        try
        {
            if (organizationId.HasValue)
            {
                await _auditService.LogValidationAttemptAsync(
                    organizationId.Value,
                    moduleCode,
                    success,
                    errorMessage);
            }
        }
        catch (Exception ex)
        {
            // Don't let audit logging failures affect the main flow
            _logger.LogError(ex, "Failed to log license validation attempt");
        }
    }
}
