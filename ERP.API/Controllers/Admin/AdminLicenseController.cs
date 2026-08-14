using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.API.Controllers.Base;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Licensing;
using ERP.Application.Common.Modules;
using Asp.Versioning;

namespace ERP.API.Controllers.Admin;

/// <summary>
/// Admin API for license and module management.
/// Only accessible by SuperAdmin.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "SuperAdmin")]
public class AdminLicenseController : BaseApiController
{
    private readonly ILicenseService _licenseService;
    private readonly IOrganizationService _organizationService;

    public AdminLicenseController(
        ILicenseService licenseService,
        IOrganizationService organizationService)
    {
        _licenseService = licenseService;
        _organizationService = organizationService;
    }

    #region License Management

    /// <summary>
    /// Get all licenses with statistics.
    /// </summary>
    [HttpGet("licenses")]
    public async Task<IActionResult> GetAllLicenses([FromQuery] bool? activeOnly = null)
    {
        var licenses = await _licenseService.GetAllLicensesAsync(activeOnly);
        var stats = await _licenseService.GetStatisticsAsync();

        return Success(new
        {
            Licenses = licenses.Select(l => new
            {
                l.Id,
                l.OrganizationId,
                OrganizationName = _organizationService.GetName(l.OrganizationId),
                Tier = l.LicenseTier.Code,
                TierDisplayName = l.LicenseTier.DisplayName,
                l.StartDate,
                l.EndDate,
                l.IsActive,
                l.IsExpired,
                l.MaxUsers
            }),
            Statistics = stats
        });
    }

    /// <summary>
    /// Get license for specific organization.
    /// </summary>
    [HttpGet("licenses/{organizationId:guid}")]
    public async Task<IActionResult> GetLicense(Guid organizationId)
    {
        var license = await _licenseService.GetLicenseAsync(organizationId);
        if (license == null)
            return NotFoundError("License not found");

        var enabledModules = await _licenseService.GetEnabledModulesAsync(organizationId);

        return Success(new
        {
            license.Id,
            license.OrganizationId,
            OrganizationName = _organizationService.GetName(organizationId),
            Tier = license.LicenseTier.Code,
            TierDisplayName = license.LicenseTier.DisplayName,
            license.StartDate,
            license.EndDate,
            license.IsActive,
            license.IsExpired,
            license.MaxUsers,
            EnabledModules = enabledModules
        });
    }

    /// <summary>
    /// Create new license for organization.
    /// </summary>
    [HttpPost("licenses")]
    public async Task<IActionResult> CreateLicense([FromBody] CreateLicenseRequest request)
    {
        try
        {
            var license = await _licenseService.CreateLicenseAsync(request);
            return Created($"/api/v1/admin/licenses/{license.OrganizationId}", new
            {
                license.Id,
                license.OrganizationId,
                OrganizationName = _organizationService.GetName(request.OrganizationId),
                Tier = license.LicenseTier.Code,
                license.StartDate,
                license.EndDate,
                license.IsActive,
                Message = $"License created successfully for tier '{license.LicenseTier.Code}'"
            });
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Update license tier.
    /// </summary>
    [HttpPut("licenses/{organizationId:guid}/tier")]
    public async Task<IActionResult> UpdateLicenseTier(Guid organizationId, [FromBody] UpdateTierRequest request)
    {
        try
        {
            var license = await _licenseService.UpdateLicenseTierAsync(organizationId, request.Tier);
            var enabledModules = await _licenseService.GetEnabledModulesAsync(organizationId);

            return Success(new
            {
                Tier = license.LicenseTier.Code,
                EnabledModules = enabledModules,
                Message = $"License updated to tier '{request.Tier}'. Modules synced."
            });
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Extend license expiration.
    /// </summary>
    [HttpPut("licenses/{organizationId:guid}/extend")]
    public async Task<IActionResult> ExtendLicense(Guid organizationId, [FromBody] ExtendLicenseRequest request)
    {
        try
        {
            var license = await _licenseService.ExtendLicenseAsync(organizationId, request.NewExpiryDate);
            return Success(new
            {
                license.EndDate,
                Message = $"License extended until {request.NewExpiryDate:yyyy-MM-dd}"
            });
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Revoke license (expire immediately).
    /// </summary>
    [HttpDelete("licenses/{organizationId:guid}")]
    public async Task<IActionResult> RevokeLicense(Guid organizationId)
    {
        try
        {
            await _licenseService.RevokeLicenseAsync(organizationId);
            return Success(new { Message = "License revoked successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
    }

    #endregion

    #region Module Management

    /// <summary>
    /// Enable module for organization.
    /// </summary>
    [HttpPost("organizations/{organizationId:guid}/modules/{moduleCode}")]
    public async Task<IActionResult> EnableModule(Guid organizationId, string moduleCode)
    {
        try
        {
            await _licenseService.EnableModuleAsync(organizationId, moduleCode.ToUpper());
            return Success(new
            {
                OrganizationId = organizationId,
                ModuleCode = moduleCode.ToUpper(),
                Message = $"Module '{moduleCode}' enabled"
            });
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Disable module for organization.
    /// </summary>
    [HttpDelete("organizations/{organizationId:guid}/modules/{moduleCode}")]
    public async Task<IActionResult> DisableModule(Guid organizationId, string moduleCode)
    {
        try
        {
            await _licenseService.DisableModuleAsync(organizationId, moduleCode.ToUpper());
            return Success(new
            {
                OrganizationId = organizationId,
                ModuleCode = moduleCode.ToUpper(),
                Message = $"Module '{moduleCode}' disabled"
            });
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Get enabled modules for organization.
    /// </summary>
    [HttpGet("organizations/{organizationId:guid}/modules")]
    public async Task<IActionResult> GetEnabledModules(Guid organizationId)
    {
        var modules = await _licenseService.GetEnabledModulesAsync(organizationId);
        var license = await _licenseService.GetLicenseAsync(organizationId);

        return Success(new
        {
            OrganizationId = organizationId,
            Tier = license?.LicenseTier.Code ?? "None",
            EnabledModules = modules,
            AllAvailableModules = ModuleConfigurationLoader.GetAllModules()
                .Select(m => new { m.Module, m.Code, m.Enabled, m.Tier })
        });
    }

    /// <summary>
    /// Sync modules based on license tier.
    /// </summary>
    [HttpPost("organizations/{organizationId:guid}/modules/sync")]
    public async Task<IActionResult> SyncModules(Guid organizationId)
    {
        await _licenseService.SyncModulesFromTierAsync(organizationId);
        var modules = await _licenseService.GetEnabledModulesAsync(organizationId);

        return Success(new
        {
            OrganizationId = organizationId,
            EnabledModules = modules,
            Message = "Modules synced from license tier"
        });
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Get license statistics.
    /// </summary>
    [HttpGet("licenses/statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        var stats = await _licenseService.GetStatisticsAsync();
        return Success(stats);
    }

    /// <summary>
    /// Get admin dashboard data.
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardData()
    {
        var stats = await _licenseService.GetStatisticsAsync();
        var licenses = await _licenseService.GetAllLicensesAsync(activeOnly: null);

        var dashboardData = new
        {
            Statistics = stats,
            ExpiringLicenses = licenses
                .Where(l => l.IsActive && (l.EndDate - DateTime.UtcNow).TotalDays <= 30)
                .OrderBy(l => l.EndDate)
                .Take(10)
                .Select(l => new
                {
                    l.OrganizationId,
                    OrganizationName = _organizationService.GetName(l.OrganizationId),
                    Tier = l.LicenseTier.Code,
                    l.EndDate,
                    DaysRemaining = (int)(l.EndDate - DateTime.UtcNow).TotalDays
                }),
            RecentOrganizations = licenses
                .OrderByDescending(l => l.StartDate)
                .Take(10)
                .Select(l => new
                {
                    l.OrganizationId,
                    OrganizationName = _organizationService.GetName(l.OrganizationId),
                    Tier = l.LicenseTier.Code,
                    l.StartDate,
                    l.EndDate,
                    l.IsActive
                }),
            TierDistribution = stats.TierDistribution
        };

        return Success(dashboardData);
    }

    #endregion
}

/// <summary>
/// Request to update license tier.
/// </summary>
public record UpdateTierRequest(string Tier);

/// <summary>
/// Request to extend license.
/// </summary>
public record ExtendLicenseRequest(DateTime NewExpiryDate);
