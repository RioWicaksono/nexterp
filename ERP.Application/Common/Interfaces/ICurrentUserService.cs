namespace ERP.Application.Common.Interfaces;

/// <summary>
/// Service to get current authenticated user context
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? OrganizationId { get; }
    string? Username { get; }
    bool IsAuthenticated { get; }
    bool IsSuperAdmin { get; }
    IEnumerable<string> Permissions { get; }

    bool HasPermission(string permission);

    /// <summary>
    /// Checks if user has ALL specified permissions
    /// </summary>
    bool HasAllPermissions(params string[] permissions);

    /// <summary>
    /// Checks if user has ANY of the specified permissions
    /// </summary>
    bool HasAnyPermission(params string[] permissions);
}
