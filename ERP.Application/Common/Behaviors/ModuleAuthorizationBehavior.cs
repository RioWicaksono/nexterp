using System.Reflection;
using MediatR;
using Microsoft.Extensions.Logging;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that checks if the user's organization has access
/// to the required module before executing the handler.
/// </summary>
public class ModuleAuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserService _currentUser;
    private readonly IModuleAccessService _moduleAccess;
    private readonly ILogger<ModuleAuthorizationBehavior<TRequest, TResponse>> _logger;

    public ModuleAuthorizationBehavior(
        ICurrentUserService currentUser,
        IModuleAccessService moduleAccess,
        ILogger<ModuleAuthorizationBehavior<TRequest, TResponse>> logger)
    {
        _currentUser = currentUser;
        _moduleAccess = moduleAccess;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Get the RequiresModule attribute from the request type
        var requiresModule = GetRequiredModule(request);

        if (requiresModule == null)
        {
            // No module requirement, proceed
            return await next();
        }

        // SuperAdmin bypasses all module checks
        if (_currentUser.IsSuperAdmin)
        {
            _logger.LogDebug("SuperAdmin bypass - Module check skipped for {ModuleCode}", requiresModule.ModuleCode);
            return await next();
        }

        // Check if user is authenticated
        if (!_currentUser.IsAuthenticated || _currentUser.OrganizationId == null)
        {
            _logger.LogWarning("Unauthorized access attempt to module {ModuleCode}", requiresModule.ModuleCode);
            return CreateForbiddenResponse(requiresModule, "User is not authenticated");
        }

        // Check organization license validity
        var isLicensed = await _moduleAccess.IsOrganizationLicensedAsync(
            _currentUser.OrganizationId.Value,
            cancellationToken);

        if (!isLicensed)
        {
            _logger.LogWarning("Organization {OrganizationId} has invalid license", _currentUser.OrganizationId);
            return CreateForbiddenResponse(requiresModule, "Organization license is expired or invalid");
        }

        // Check module access
        var hasAccess = await _moduleAccess.HasModuleAccessAsync(
            _currentUser.OrganizationId.Value,
            requiresModule.ModuleCode,
            cancellationToken);

        if (!hasAccess)
        {
            _logger.LogWarning(
                "Organization {OrganizationId} does not have access to module {ModuleCode}",
                _currentUser.OrganizationId,
                requiresModule.ModuleCode);

            return CreateForbiddenResponse(
                requiresModule,
                $"Module '{requiresModule.ModuleCode}' is not activated for your organization");
        }

        _logger.LogDebug(
            "Module access granted for organization {OrganizationId} to module {ModuleCode}",
            _currentUser.OrganizationId,
            requiresModule.ModuleCode);

        return await next();
    }

    private static RequiresModuleAttribute? GetRequiredModule(TRequest request)
    {
        // Check on the request type (class-level attribute)
        var type = request.GetType();
        var attribute = type.GetCustomAttribute<RequiresModuleAttribute>(inherit: true);

        if (attribute != null)
            return attribute;

        // Also check on MediatR handler methods (for method-level attributes)
        // This is less common but supported
        return null;
    }

    private TResponse CreateForbiddenResponse(RequiresModuleAttribute requiresModule, string reason)
    {
        // Try to create a Result.Failure response
        var resultType = typeof(TResponse);

        // Check if TResponse is Result or Result<T>
        if (resultType == typeof(Application.Common.Base.Result))
        {
            var failureMethod = typeof(Application.Common.Base.Result)
                .GetMethod(nameof(Application.Common.Base.Result.Failure), 1, new[] { typeof(string) });

            return (TResponse)(failureMethod?.Invoke(null, new object[] { requiresModule.ErrorMessage ?? reason })!);
        }

        // For Result<T> or other types, we need to throw an exception
        // This will be caught by the global exception handler
        throw new ModuleAccessDeniedException(requiresModule.ModuleCode, reason, requiresModule.ErrorMessage);
    }
}

/// <summary>
/// Exception thrown when module access is denied.
/// </summary>
public class ModuleAccessDeniedException : Exception
{
    public string ModuleCode { get; }
    public string Reason { get; }

    public ModuleAccessDeniedException(string moduleCode, string reason, string? userMessage = null)
        : base(userMessage ?? $"Access denied to module '{moduleCode}': {reason}")
    {
        ModuleCode = moduleCode;
        Reason = reason;
    }
}
