using ERP.Domain.Common.Modules;
using MediatR;

namespace ERP.Application.Common.Behaviors;

/// <summary>
/// Attribute to specify required module for a command/query.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireModuleAttribute : Attribute
{
    public string ModuleCode { get; }
    public string? FeatureName { get; }

    public RequireModuleAttribute(string moduleCode, string? featureName = null)
    {
        ModuleCode = moduleCode;
        FeatureName = featureName;
    }
}

/// <summary>
/// License validation result.
/// </summary>
public record LicenseValidationResult(bool IsValid, string? ErrorMessage, string? ModuleCode);

/// <summary>
/// Pipeline behavior to validate license before processing requests.
/// </summary>
public class LicenseValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILicenseCheckService _licenseCheck;

    public LicenseValidationBehavior(ILicenseCheckService licenseCheck)
    {
        _licenseCheck = licenseCheck;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Get module requirement from attribute
        var moduleAttr = request?.GetType()
            .GetCustomAttributes(typeof(RequireModuleAttribute), true)
            .FirstOrDefault() as RequireModuleAttribute;

        if (moduleAttr != null)
        {
            var validation = await _licenseCheck.ValidateModuleAccessAsync(
                moduleAttr.ModuleCode,
                moduleAttr.FeatureName);

            if (!validation.IsValid)
            {
                throw new LicenseValidationException(
                    validation.ErrorMessage ?? "Module access denied",
                    validation.ModuleCode);
            }
        }

        return await next();
    }
}

/// <summary>
/// Exception thrown when license validation fails.
/// </summary>
public class LicenseValidationException : Exception
{
    public string ModuleCode { get; }

    public LicenseValidationException(string message, string? moduleCode = null)
        : base(message)
    {
        ModuleCode = moduleCode ?? "UNKNOWN";
    }
}

/// <summary>
/// Service to check license/module access.
/// </summary>
public interface ILicenseCheckService
{
    /// <summary>
    /// Validate if current organization has access to module.
    /// </summary>
    Task<LicenseValidationResult> ValidateModuleAccessAsync(string moduleCode, string? featureName = null);

    /// <summary>
    /// Check if license is valid and not expired.
    /// </summary>
    Task<bool> IsLicenseValidAsync();
}
