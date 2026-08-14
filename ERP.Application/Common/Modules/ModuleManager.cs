using ERP.Application.Common.Interfaces;
using ERP.Domain.Common.Modules;

namespace ERP.Application.Common.Modules;

/// <summary>
/// Module manager service for checking and managing module availability.
/// </summary>
public interface IModuleManager
{
    /// <summary>
    /// Check if a module is enabled.
    /// </summary>
    bool IsModuleEnabled(string moduleCode);

    /// <summary>
    /// Check if a feature is enabled within a module.
    /// </summary>
    bool IsFeatureEnabled(string moduleCode, string featureName);

    /// <summary>
    /// Get all enabled modules for a license tier.
    /// </summary>
    List<string> GetEnabledModules(string tier);

    /// <summary>
    /// Get module configuration.
    /// </summary>
    ModuleConfiguration GetModuleConfig(string moduleCode);

    /// <summary>
    /// Check if organization has access to module.
    /// </summary>
    Task<bool> HasModuleAccessAsync(Guid organizationId, string moduleCode);
}

/// <summary>
/// Module manager implementation.
/// </summary>
public class ModuleManager : IModuleManager
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ModuleManager(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public bool IsModuleEnabled(string moduleCode)
    {
        // SuperAdmin bypasses all checks
        if (_currentUser.IsSuperAdmin)
            return true;

        return ModuleConfigurationLoader.IsModuleEnabled(moduleCode);
    }

    public bool IsFeatureEnabled(string moduleCode, string featureName)
    {
        // SuperAdmin bypasses all checks
        if (_currentUser.IsSuperAdmin)
            return true;

        return ModuleConfigurationLoader.IsFeatureEnabled(moduleCode, featureName);
    }

    public List<string> GetEnabledModules(string tier)
    {
        var manifest = ModuleConfigurationLoader.LoadManifest();
        var tierInfo = manifest.Tiers.GetValueOrDefault(tier.ToLower());

        if (tierInfo == null)
            return new List<string>();

        return tierInfo.Modules
            .Where(m => manifest.Modules.GetValueOrDefault(m)?.Enabled ?? false)
            .ToList();
    }

    public ModuleConfiguration GetModuleConfig(string moduleCode)
    {
        return ModuleConfigurationLoader.LoadModuleConfig(moduleCode.ToLower());
    }

    public async Task<bool> HasModuleAccessAsync(Guid organizationId, string moduleCode)
    {
        // SuperAdmin bypasses all checks
        if (_currentUser.IsSuperAdmin)
            return true;

        // Check if module is globally enabled
        if (!IsModuleEnabled(moduleCode))
            return false;

        // Check if organization has an active license
        var hasLicense = await Task.FromResult(_context.OrganizationLicenses
            .Any(l => l.OrganizationId == organizationId && l.IsActive));

        if (!hasLicense)
            return false;

        // Check if organization has this specific module enabled
        // Get all active modules for the organization
        var orgModules = _context.OrganizationModules
            .Where(m => m.OrganizationId == organizationId && !m.IsExpired)
            .Select(m => m.ModuleId)
            .ToList();

        // Get the module definition ID
        var moduleDef = _context.Modules
            .FirstOrDefault(m => m.Code == moduleCode);

        if (moduleDef == null)
            return false;

        return orgModules.Contains(moduleDef.Id);
    }
}
