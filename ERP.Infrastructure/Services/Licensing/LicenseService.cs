using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Licensing;
using ERP.Domain.Common.Modules;

namespace ERP.Infrastructure.Services;

/// <summary>
/// License management service implementation.
/// </summary>
public class LicenseService : ILicenseService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public LicenseService(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<OrganizationLicense?> GetLicenseAsync(Guid organizationId)
    {
        return await Task.FromResult(
            _context.OrganizationLicenses
                .FirstOrDefault(l => l.OrganizationId == organizationId));
    }

    public async Task<OrganizationLicense> CreateLicenseAsync(CreateLicenseRequest request)
    {
        // Check if license already exists
        var existing = _context.OrganizationLicenses
            .FirstOrDefault(l => l.OrganizationId == request.OrganizationId);

        if (existing != null)
        {
            throw new InvalidOperationException("License already exists for this organization");
        }

        // Get tier ID
        var tier = _context.LicenseTiers
            .FirstOrDefault(t => t.Code.ToUpper() == request.Tier.ToUpper());

        if (tier == null)
        {
            throw new InvalidOperationException($"Tier '{request.Tier}' not found");
        }

        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(request.DurationDays);

        var license = new OrganizationLicense(
            request.OrganizationId,
            tier.Id,
            startDate,
            endDate,
            request.DurationDays, // Using DurationDays as MaxUsers
            request.Notes,
            false);

        _context.OrganizationLicenses.Add(license);
        await _context.SaveChangesAsync();

        // Sync modules based on tier
        await SyncModulesFromTierAsync(request.OrganizationId);

        return license;
    }

    public async Task<OrganizationLicense> UpdateLicenseTierAsync(Guid organizationId, string tier)
    {
        var license = await GetLicenseAsync(organizationId);
        if (license == null)
            throw new InvalidOperationException("License not found");

        var newTier = _context.LicenseTiers
            .FirstOrDefault(t => t.Code.ToUpper() == tier.ToUpper());

        if (newTier == null)
            throw new InvalidOperationException($"Tier '{tier}' not found");

        // Create new license with new tier
        var newLicense = new OrganizationLicense(
            organizationId,
            newTier.Id,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(365),
            365,
            $"Upgraded from previous tier on {DateTime.UtcNow:yyyy-MM-dd}",
            false);

        // Mark old as expired
        license.Renew(DateTime.UtcNow.AddDays(-1));

        _context.OrganizationLicenses.Add(newLicense);
        await _context.SaveChangesAsync();

        // Sync modules based on new tier
        await SyncModulesFromTierAsync(organizationId);

        return newLicense;
    }

    public async Task<OrganizationLicense> ExtendLicenseAsync(Guid organizationId, DateTime newExpiry)
    {
        var license = await GetLicenseAsync(organizationId);
        if (license == null)
            throw new InvalidOperationException("License not found");

        license.Renew(newExpiry);
        await _context.SaveChangesAsync();

        return license;
    }

    public async Task RevokeLicenseAsync(Guid organizationId)
    {
        var license = await GetLicenseAsync(organizationId);
        if (license == null)
            throw new InvalidOperationException("License not found");

        // Set end date to yesterday to expire immediately
        license.Renew(DateTime.UtcNow.AddDays(-1));
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsLicenseValidAsync(Guid organizationId)
    {
        var license = await GetLicenseAsync(organizationId);
        return license?.IsActive ?? false;
    }

    public async Task<List<OrganizationLicense>> GetAllLicensesAsync(bool? activeOnly = null)
    {
        return await Task.FromResult(
            _context.OrganizationLicenses
                .Where(l => activeOnly != true || l.IsActive)
                .ToList());
    }

    public async Task<LicenseStatistics> GetStatisticsAsync()
    {
        var all = _context.OrganizationLicenses.ToList();
        var now = DateTime.UtcNow;

        var stats = new LicenseStatistics(
            TotalOrganizations: all.Select(l => l.OrganizationId).Distinct().Count(),
            ActiveLicenses: all.Count(l => l.IsActive),
            ExpiredLicenses: all.Count(l => l.IsExpired),
            ExpiringIn7Days: all.Count(l => l.IsActive && (l.EndDate - now).TotalDays <= 7),
            ExpiringIn30Days: all.Count(l => l.IsActive && (l.EndDate - now).TotalDays <= 30),
            TierDistribution: all
                .Where(l => l.LicenseTier != null)
                .GroupBy(l => l.LicenseTier.Code)
                .ToDictionary(g => g.Key, g => g.Count()));

        return stats;
    }

    public async Task<bool> HasModuleAccessAsync(Guid organizationId, string moduleCode)
    {
        // Check license validity
        if (!await IsLicenseValidAsync(organizationId))
            return false;

        // Get enabled modules for organization
        var enabledModules = await GetEnabledModulesAsync(organizationId);
        return enabledModules.Contains(moduleCode, StringComparer.OrdinalIgnoreCase);
    }

    public async Task EnableModuleAsync(Guid organizationId, string moduleCode)
    {
        var moduleDef = _context.Modules.FirstOrDefault(m => m.Code == moduleCode);
        if (moduleDef == null)
            throw new InvalidOperationException($"Module '{moduleCode}' not found");

        var existing = _context.OrganizationModules
            .FirstOrDefault(m => m.OrganizationId == organizationId && m.ModuleId == moduleDef.Id);

        if (existing != null)
        {
            if (existing.IsExpired)
            {
                existing.Extend(DateTime.UtcNow.AddYears(1), _currentUser.Username ?? "System");
            }
        }
        else
        {
            var orgModule = new OrganizationModule(
                organizationId,
                moduleDef.Id,
                _currentUser.Username,
                DateTime.UtcNow.AddYears(1));

            _context.OrganizationModules.Add(orgModule);
        }

        await _context.SaveChangesAsync();
    }

    public async Task DisableModuleAsync(Guid organizationId, string moduleCode)
    {
        var moduleDef = _context.Modules.FirstOrDefault(m => m.Code == moduleCode);
        if (moduleDef == null)
            throw new InvalidOperationException($"Module '{moduleCode}' not found");

        var orgModule = _context.OrganizationModules
            .FirstOrDefault(m => m.OrganizationId == organizationId && m.ModuleId == moduleDef.Id);

        if (orgModule != null && !orgModule.IsExpired)
        {
            orgModule.Revoke(_currentUser.Username ?? "System", "Manual disable");
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<string>> GetEnabledModulesAsync(Guid organizationId)
    {
        return _context.OrganizationModules
            .Where(m => m.OrganizationId == organizationId && !m.IsExpired)
            .Select(m => m.Module.Code)
            .ToList();
    }

    public async Task SyncModulesFromTierAsync(Guid organizationId)
    {
        var license = await GetLicenseAsync(organizationId);
        if (license == null)
            return;

        var tierModules = GetTierModules(license.LicenseTier?.Code ?? "STARTER");

        // Disable modules not in tier
        var currentModules = _context.OrganizationModules
            .Where(m => m.OrganizationId == organizationId && !m.IsExpired)
            .ToList();

        foreach (var module in currentModules)
        {
            if (!tierModules.Contains(module.Module.Code, StringComparer.OrdinalIgnoreCase))
            {
                module.Revoke(_currentUser.Username ?? "System", $"Removed from tier '{license.LicenseTier?.Code}'");
            }
        }

        // Enable modules in tier
        foreach (var moduleCode in tierModules)
        {
            await EnableModuleAsync(organizationId, moduleCode);
        }

        await _context.SaveChangesAsync();
    }

    private static List<string> GetTierModules(string tier)
    {
        return tier.ToUpper() switch
        {
            "STARTER" => new List<string> { "SALES", "INVENTORY", "PURCHASING" },
            "PROFESSIONAL" => new List<string> { "SALES", "INVENTORY", "PURCHASING", "HRM", "ACCOUNTING" },
            "ENTERPRISE" => new List<string>
            {
                "SALES", "INVENTORY", "PURCHASING", "HRM", "ACCOUNTING",
                "PROJECTS", "QUALITY", "ASSETS", "ANALYTICS"
            },
            _ => new List<string>()
        };
    }
}
