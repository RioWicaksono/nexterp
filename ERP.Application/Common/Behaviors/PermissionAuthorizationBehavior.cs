using System.Reflection;
using System.Security.Claims;
using MediatR;
using Microsoft.Extensions.Logging;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that checks if the user has required permission(s)
/// before executing the handler. Supports both class-level and method-level attributes.
/// </summary>
public class PermissionAuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<PermissionAuthorizationBehavior<TRequest, TResponse>> _logger;

    public PermissionAuthorizationBehavior(
        ICurrentUserService currentUser,
        ILogger<PermissionAuthorizationBehavior<TRequest, TResponse>> logger)
    {
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Get the RequiresPermission attribute from the request type
        var requiresPermission = GetRequiredPermission(request);

        if (requiresPermission == null)
        {
            // No permission requirement, proceed
            return await next();
        }

        // SuperAdmin bypasses all permission checks
        if (_currentUser.IsSuperAdmin)
        {
            _logger.LogDebug("SuperAdmin bypass - Permission check skipped for {Permission}", requiresPermission.Permission);
            return await next();
        }

        // Check if user is authenticated
        if (!_currentUser.IsAuthenticated)
        {
            _logger.LogWarning("Unauthorized access attempt - no authentication");
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        // Check permission based on requirement type
        bool hasPermission = requiresPermission.RequirementType switch
        {
            PermissionRequirementType.RequireAll => _currentUser.HasAllPermissions(requiresPermission.Permissions),
            PermissionRequirementType.RequireAny => _currentUser.HasAnyPermission(requiresPermission.Permissions),
            _ => _currentUser.HasPermission(requiresPermission.Permission!)
        };

        if (!hasPermission)
        {
            var requiredPerms = string.Join(", ", requiresPermission.Permissions);
            _logger.LogWarning(
                "Permission denied for user {UserId}. Required: {Required}, User permissions: {UserPermissions}",
                _currentUser.UserId,
                requiredPerms,
                string.Join(", ", _currentUser.Permissions));

            throw new PermissionDeniedException(
                requiredPerms,
                "You do not have the required permissions to perform this action");
        }

        var logPermission = string.Join(", ", requiresPermission.Permissions);
        _logger.LogDebug(
            "Permission granted for user {UserId}: {Permission}",
            _currentUser.UserId,
            logPermission);

        return await next();
    }

    private static RequiresPermissionAttribute? GetRequiredPermission(TRequest request)
    {
        var type = request.GetType();

        // Check class-level attribute first
        var attribute = type.GetCustomAttribute<RequiresPermissionAttribute>(inherit: true);
        return attribute;
    }
}

/// <summary>
/// Specifies the type of permission requirement
/// </summary>
public enum PermissionRequirementType
{
    /// <summary>User must have the single specified permission</summary>
    RequireSingle,

    /// <summary>User must have ALL specified permissions</summary>
    RequireAll,

    /// <summary>User must have ANY of the specified permissions</summary>
    RequireAny
}

/// <summary>
/// Attribute to specify required permission(s) for a request handler.
/// Can be applied at class level on MediatR IRequest classes.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class RequiresPermissionAttribute : Attribute
{
    /// <summary>Single permission required (use with RequireSingle)</summary>
    public string? Permission { get; }

    /// <summary>Multiple permissions (use with RequireAll or RequireAny)</summary>
    public string[] Permissions { get; }

    /// <summary>How multiple permissions should be evaluated</summary>
    public PermissionRequirementType RequirementType { get; }

    /// <summary>
    /// Creates a single permission requirement
    /// </summary>
    public RequiresPermissionAttribute(string permission)
    {
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
        Permissions = Array.Empty<string>();
        RequirementType = PermissionRequirementType.RequireSingle;
    }

    /// <summary>
    /// Creates multiple permission requirements with specified evaluation type
    /// </summary>
    public RequiresPermissionAttribute(PermissionRequirementType requirementType, params string[] permissions)
    {
        if (permissions == null || permissions.Length == 0)
            throw new ArgumentException("At least one permission is required", nameof(permissions));

        Permissions = permissions;
        RequirementType = requirementType;
        Permission = null;
    }
}

/// <summary>
/// Exception thrown when permission check fails
/// </summary>
public class PermissionDeniedException : Exception
{
    public IEnumerable<string> RequiredPermissions { get; }

    public PermissionDeniedException(string requiredPermissions, string message)
        : base(message)
    {
        RequiredPermissions = requiredPermissions.Split(',', StringSplitOptions.TrimEntries);
    }

    public PermissionDeniedException(IEnumerable<string> requiredPermissions, string message)
        : base(message)
    {
        RequiredPermissions = requiredPermissions;
    }
}
