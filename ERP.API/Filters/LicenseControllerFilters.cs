using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ERP.Application.Common.Behaviors;

namespace ERP.API.Filters;

/// <summary>
/// Action filter attribute to check license at controller level.
/// This provides a second layer of defense against bypass attempts.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireLicenseAttribute : Attribute, IAsyncActionFilter
{
    public string ModuleCode { get; }
    public string? FeatureName { get; }

    public RequireLicenseAttribute(string moduleCode, string? featureName = null)
    {
        ModuleCode = moduleCode;
        FeatureName = featureName;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var licenseCheck = context.HttpContext.RequestServices.GetService<ILicenseCheckService>();
        var logger = context.HttpContext.RequestServices.GetService<ILogger<RequireLicenseAttribute>>();

        if (licenseCheck == null)
        {
            logger?.LogError("ILicenseCheckService not registered - license validation bypassed!");
            context.Result = new ObjectResult(new
            {
                error = "License validation service unavailable",
                code = "SERVICE_UNAVAILABLE"
            })
            {
                StatusCode = StatusCodes.Status503ServiceUnavailable
            };
            return;
        }

        try
        {
            var validation = await licenseCheck.ValidateModuleAccessAsync(ModuleCode, FeatureName);

            if (!validation.IsValid)
            {
                logger?.LogWarning(
                    "Controller-level license check failed for module {Module} - {Message}",
                    ModuleCode,
                    validation.ErrorMessage);

                context.Result = new ObjectResult(new
                {
                    error = validation.ErrorMessage ?? "License not valid",
                    code = "LICENSE_INVALID",
                    module = ModuleCode
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            // Log successful validation
            logger?.LogInformation(
                "Controller-level license check passed for module {Module}",
                ModuleCode);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error during controller-level license check for module {Module}", ModuleCode);
            context.Result = new ObjectResult(new
            {
                error = "License validation error",
                code = "VALIDATION_ERROR"
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            return;
        }

        await next();
    }
}

/// <summary>
/// Action filter attribute to require valid license (any tier).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireValidLicenseAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var licenseCheck = context.HttpContext.RequestServices.GetService<ILicenseCheckService>();
        var logger = context.HttpContext.RequestServices.GetService<ILogger<RequireValidLicenseAttribute>>();

        if (licenseCheck == null)
        {
            context.Result = new ObjectResult(new
            {
                error = "License validation service unavailable",
                code = "SERVICE_UNAVAILABLE"
            })
            {
                StatusCode = StatusCodes.Status503ServiceUnavailable
            };
            return;
        }

        try
        {
            var isValid = await licenseCheck.IsLicenseValidAsync();

            if (!isValid)
            {
                logger?.LogWarning("Controller-level license validity check failed");
                context.Result = new ObjectResult(new
                {
                    error = "License is not valid or has expired",
                    code = "LICENSE_INVALID"
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error during license validity check");
            context.Result = new ObjectResult(new
            {
                error = "License validation error",
                code = "VALIDATION_ERROR"
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            return;
        }

        await next();
    }
}

/// <summary>
/// Base controller with built-in license validation.
/// Inherit from this instead of ControllerBase for license-protected controllers.
/// </summary>
public abstract class LicenseProtectedControllerBase : ControllerBase
{
    private ILicenseCheckService? _licenseCheck;
    private ILogger? _logger;

    protected ILicenseCheckService LicenseCheck =>
        _licenseCheck ??= HttpContext.RequestServices.GetRequiredService<ILicenseCheckService>();

    protected ILogger Logger =>
        _logger ??= HttpContext.RequestServices.GetRequiredService<ILogger>();

    /// <summary>
    /// Validates that the current organization has access to the specified module.
    /// </summary>
    protected async Task<bool> ValidateModuleAccessAsync(string moduleCode, string? featureName = null)
    {
        var result = await LicenseCheck.ValidateModuleAccessAsync(moduleCode, featureName);

        if (!result.IsValid)
        {
            Logger.LogWarning(
                "Module access denied: {Module} - {Message}",
                moduleCode,
                result.ErrorMessage);

            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates module access and returns appropriate error response if invalid.
    /// </summary>
    protected async Task<IActionResult> ValidateOrFail(string moduleCode, string? featureName = null)
    {
        if (!await ValidateModuleAccessAsync(moduleCode, featureName))
        {
            return Forbid("License not valid or module not enabled");
        }

        return Ok();
    }

    /// <summary>
    /// Validates that the organization has a valid license.
    /// </summary>
    protected async Task<bool> ValidateLicenseAsync()
    {
        return await LicenseCheck.IsLicenseValidAsync();
    }

    /// <summary>
    /// Validates license and returns appropriate error response if invalid.
    /// </summary>
    protected async Task<IActionResult> ValidateLicenseOrFail()
    {
        if (!await ValidateLicenseAsync())
        {
            return Forbid("License is not valid or has expired");
        }

        return Ok();
    }
}
