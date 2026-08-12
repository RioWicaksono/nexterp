using ERP.Domain.Common;

namespace ERP.Domain.Base;

/// <summary>
/// Role entity for RBAC (Role-Based Access Control)
/// </summary>
public class Role : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsSystemRole { get; private set; }

    // Navigation properties
    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private readonly List<RolePermission> _permissions = new();
    public IReadOnlyCollection<RolePermission> Permissions => _permissions.AsReadOnly();

    // Factory method
    public static Role Create(
        Guid organizationId,
        string name,
        string? description = null,
        bool isSystemRole = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required", nameof(name));

        return new Role
        {
            OrganizationId = organizationId,
            Name = name.Trim(),
            Description = description?.Trim(),
            IsSystemRole = isSystemRole
        };
    }

    public void Update(string? name = null, string? description = null)
    {
        if (IsSystemRole)
            throw new InvalidOperationException("Cannot modify system role");

        Name = name?.Trim() ?? Name;
        Description = description?.Trim() ?? Description;
        UpdateTimestamp();
    }

    public void Activate() { IsActive = true; UpdateTimestamp(); }
    public void Deactivate()
    {
        if (IsSystemRole)
            throw new InvalidOperationException("Cannot deactivate system role");
        IsActive = false;
        UpdateTimestamp();
    }

    public void AddPermission(string permission)
    {
        if (IsSystemRole)
            throw new InvalidOperationException("Cannot modify system role permissions");

        if (!_permissions.Any(p => p.Permission == permission))
        {
            _permissions.Add(RolePermission.Create(this.Id, permission));
            UpdateTimestamp();
        }
    }

    public void RemovePermission(string permission)
    {
        if (IsSystemRole)
            throw new InvalidOperationException("Cannot modify system role permissions");

        var rolePermission = _permissions.FirstOrDefault(p => p.Permission == permission);
        if (rolePermission != null)
        {
            _permissions.Remove(rolePermission);
            UpdateTimestamp();
        }
    }

    public bool HasPermission(string permission) =>
        _permissions.Any(p => p.Permission == permission);

    public void ClearPermissions()
    {
        if (IsSystemRole)
            throw new InvalidOperationException("Cannot modify system role permissions");

        _permissions.Clear();
        UpdateTimestamp();
    }
}

/// <summary>
/// Join table for User-Role many-to-many relationship
/// </summary>
public class UserRole : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }

    // Navigation properties
    private readonly User? _user;
    public User? User => _user;

    private readonly Role? _role;
    public Role? Role => _role;

    public static UserRole Create(Guid userId, Guid roleId) => new()
    {
        UserId = userId,
        RoleId = roleId
    };
}

/// <summary>
/// Join table for Role-Permission many-to-many relationship
/// </summary>
public class RolePermission : BaseEntity
{
    public Guid RoleId { get; private set; }
    public string Permission { get; private set; } = string.Empty;

    private readonly Role? _role;
    public Role? Role => _role;

    public static RolePermission Create(Guid roleId, string permission) => new()
    {
        RoleId = roleId,
        Permission = permission
    };
}
