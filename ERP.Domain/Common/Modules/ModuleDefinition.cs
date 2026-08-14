using ERP.Domain.Base;

namespace ERP.Domain.Common.Modules;

/// <summary>
/// Represents an available module in the ERP system.
/// Each module can be licensed independently per organization.
/// </summary>
public class ModuleDefinition : BaseEntity
{
    public string Code { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ModuleCategory Category { get; private set; }
    public bool IsPremium { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<OrganizationModule> _organizationModules = new();
    public IReadOnlyCollection<OrganizationModule> OrganizationModules => _organizationModules.AsReadOnly();

    private readonly List<ModulePermission> _permissions = new();
    public IReadOnlyCollection<ModulePermission> Permissions => _permissions.AsReadOnly();

    // Required for EF Core
    private ModuleDefinition() { }

    public ModuleDefinition(string code, string displayName, ModuleCategory category, bool isPremium = false, string? description = null, int sortOrder = 0)
    {
        Code = code.ToUpperInvariant();
        DisplayName = displayName;
        Category = category;
        IsPremium = isPremium;
        Description = description;
        SortOrder = sortOrder;
    }

    public void UpdateDisplayName(string displayName) => DisplayName = displayName;
    public void UpdateDescription(string? description) => Description = description;
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
