using ERP.Domain.Base;

namespace ERP.Domain.Common.Modules;

/// <summary>
/// Links modules to their associated permissions.
/// Used for module-level permission grouping.
/// </summary>
public class ModulePermission : BaseEntity
{
    public Guid ModuleId { get; private set; }
    public string Permission { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    // Navigation property
    private readonly ModuleDefinition _module = null!;
    public ModuleDefinition Module => _module;

    // Required for EF Core
    private ModulePermission() { }

    public ModulePermission(Guid moduleId, string permission, string? description = null)
    {
        ModuleId = moduleId;
        Permission = permission;
        Description = description;
    }
}
