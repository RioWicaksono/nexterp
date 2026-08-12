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
}
