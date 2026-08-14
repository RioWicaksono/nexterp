using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Common.Modules;
using ERP.Domain.Common.Configuration;

namespace ERP.Infrastructure.Services;

public class ModuleAccessService : IModuleAccessService
{
    private readonly IApplicationDbContext _context;

    public ModuleAccessService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasModuleAccessAsync(Guid organizationId, string moduleCode, CancellationToken cancellationToken = default)
    {
        var moduleCodeUpper = moduleCode.ToUpperInvariant();

        // First check if organization has a valid license
        var hasValidLicense = await IsOrganizationLicensedAsync(organizationId, cancellationToken);
        if (!hasValidLicense)
            return false;

        // Then check if the module is activated for this organization
        // Use explicit Include to ensure navigation property is loaded and N+1 is avoided
        var isActivated = await _context.Set<OrganizationModule>()
            .Include(om => om.Module)
            .AnyAsync(om =>
                om.OrganizationId == organizationId &&
                om.Module.Code == moduleCodeUpper &&
                !om.IsExpired,
                cancellationToken);

        return isActivated;
    }

    public async Task<bool> IsOrganizationLicensedAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var license = await _context.Set<OrganizationLicense>()
            .Include(l => l.LicenseTier)
            .FirstOrDefaultAsync(l => l.OrganizationId == organizationId && l.IsActive, cancellationToken);

        return license != null;
    }

    public async Task<IEnumerable<string>> GetActivatedModulesAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<OrganizationModule>()
            .Where(om => om.OrganizationId == organizationId && !om.IsExpired)
            .Include(om => om.Module)
            .Select(om => om.Module.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid> ActivateModuleAsync(Guid organizationId, string moduleCode, string? activatedBy = null, DateTime? expiresAt = null, CancellationToken cancellationToken = default)
    {
        var module = await _context.Set<ModuleDefinition>()
            .FirstOrDefaultAsync(m => m.Code == moduleCode.ToUpperInvariant(), cancellationToken)
            ?? throw new InvalidOperationException($"Module '{moduleCode}' not found");

        // Check if already activated
        var existing = await _context.Set<OrganizationModule>()
            .FirstOrDefaultAsync(om =>
                om.OrganizationId == organizationId &&
                om.ModuleId == module.Id &&
                !om.IsExpired,
                cancellationToken);

        if (existing != null)
            throw new InvalidOperationException($"Module '{moduleCode}' is already activated for this organization");

        var orgModule = new OrganizationModule(organizationId, module.Id, activatedBy, expiresAt);
        _context.Set<OrganizationModule>().Add(orgModule);

        await _context.SaveChangesAsync(cancellationToken);
        return orgModule.Id;
    }

    public async Task DeactivateModuleAsync(Guid organizationId, string moduleCode, string? deactivatedBy = null, string? reason = null, CancellationToken cancellationToken = default)
    {
        var module = await _context.Set<ModuleDefinition>()
            .FirstOrDefaultAsync(m => m.Code == moduleCode.ToUpperInvariant(), cancellationToken)
            ?? throw new InvalidOperationException($"Module '{moduleCode}' not found");

        var orgModule = await _context.Set<OrganizationModule>()
            .FirstOrDefaultAsync(om =>
                om.OrganizationId == organizationId &&
                om.ModuleId == module.Id &&
                !om.IsExpired,
                cancellationToken)
            ?? throw new InvalidOperationException($"Module '{moduleCode}' is not activated for this organization");

        orgModule.Revoke(deactivatedBy ?? "SYSTEM", reason ?? "Manual deactivation");

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<LicenseTier?> GetLicenseTierAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var license = await _context.Set<OrganizationLicense>()
            .Include(l => l.LicenseTier)
            .FirstOrDefaultAsync(l => l.OrganizationId == organizationId && l.IsActive, cancellationToken);

        return license?.LicenseTier;
    }

    public async Task<bool> HasUserCapacityAsync(Guid organizationId, int additionalUsers = 0, CancellationToken cancellationToken = default)
    {
        var license = await _context.Set<OrganizationLicense>()
            .FirstOrDefaultAsync(l => l.OrganizationId == organizationId && l.IsActive, cancellationToken);

        if (license == null)
            return false;

        var currentUserCount = await _context.Set<Domain.Base.User>()
            .CountAsync(u => u.OrganizationId == organizationId && !u.IsDeleted, cancellationToken);

        return (currentUserCount + additionalUsers) <= license.MaxUsers;
    }

    public async Task<T?> GetSettingAsync<T>(Guid organizationId, string settingKey, T? defaultValue = default, CancellationToken cancellationToken = default)
    {
        var setting = await _context.Set<OrganizationSetting>()
            .FirstOrDefaultAsync(s =>
                s.OrganizationId == organizationId &&
                s.SettingKey == settingKey,
                cancellationToken);

        if (setting == null)
            return defaultValue;

        if (typeof(T) == typeof(string))
            return (T)(object)setting.SettingValue;

        if (typeof(T) == typeof(int))
        {
            if (int.TryParse(setting.SettingValue, out var intValue))
                return (T)(object)intValue;
            return defaultValue;
        }

        if (typeof(T) == typeof(decimal))
        {
            if (decimal.TryParse(setting.SettingValue, out var decimalValue))
                return (T)(object)decimalValue;
            return defaultValue;
        }

        if (typeof(T) == typeof(bool))
        {
            if (bool.TryParse(setting.SettingValue, out var boolValue))
                return (T)(object)boolValue;
            return defaultValue;
        }

        // For complex types, assume JSON
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(setting.SettingValue);
        }
        catch
        {
            return defaultValue;
        }
    }

    public async Task SetSettingAsync(Guid organizationId, string settingKey, string value, string category, string? description = null, bool isEncrypted = false, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Set<OrganizationSetting>()
            .FirstOrDefaultAsync(s =>
                s.OrganizationId == organizationId &&
                s.SettingKey == settingKey,
                cancellationToken);

        if (existing != null)
        {
            // If setting is already encrypted, we cannot change it via UpdateValue
            if (existing.IsEncrypted)
            {
                throw new InvalidOperationException("Cannot update an encrypted setting. Use a separate method for encrypted value updates.");
            }
            existing.UpdateValue(value);
        }
        else
        {
            var setting = new OrganizationSetting(organizationId, settingKey, value, category, description, isEncrypted);
            _context.Set<OrganizationSetting>().Add(setting);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
