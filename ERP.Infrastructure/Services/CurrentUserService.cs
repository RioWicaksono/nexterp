using Microsoft.AspNetCore.Http;
using ERP.Domain.Base;

namespace ERP.Infrastructure.Services;

/// <summary>
/// Current user service implementation with permission checking
/// </summary>
public class CurrentUserService : ERP.Application.Common.Interfaces.ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("uid")?.Value;
            return Guid.TryParse(userId, out var id) ? id : null;
        }
    }

    public Guid? OrganizationId
    {
        get
        {
            var orgId = _httpContextAccessor.HttpContext?.User?.FindFirst("org")?.Value;
            return Guid.TryParse(orgId, out var id) ? id : null;
        }
    }

    public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirst("unm")?.Value;

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool IsSuperAdmin
    {
        get
        {
            var isSuper = _httpContextAccessor.HttpContext?.User?.FindFirst("sadm")?.Value;
            return isSuper == "true";
        }
    }

    public IEnumerable<string> Permissions
    {
        get
        {
            var permissions = _httpContextAccessor.HttpContext?.User?.FindFirst("per")?.Value;
            if (string.IsNullOrEmpty(permissions))
                return Enumerable.Empty<string>();
            return permissions.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }
    }

    public bool HasPermission(string permission)
    {
        if (IsSuperAdmin) return true;
        return Permissions.Contains(permission);
    }

    public bool HasAllPermissions(params string[] permissions)
    {
        if (IsSuperAdmin) return true;
        return permissions.All(p => Permissions.Contains(p));
    }

    public bool HasAnyPermission(params string[] permissions)
    {
        if (IsSuperAdmin) return true;
        return permissions.Any(p => Permissions.Contains(p));
    }
}
