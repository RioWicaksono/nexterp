using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ERP.Application.Common.Modules;

namespace ERP.API.Filters;

/// <summary>
/// Attribute to require a specific module to be enabled.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireModuleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _moduleCode;

    public RequireModuleAttribute(string moduleCode)
    {
        _moduleCode = moduleCode;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var moduleManager = context.HttpContext.RequestServices.GetRequiredService<IModuleManager>();

        if (!moduleManager.IsModuleEnabled(_moduleCode))
        {
            context.Result = new ObjectResult(new
            {
                Success = false,
                Error = $"Module '{_moduleCode}' is not available. Please contact administrator."
            })
            {
                StatusCode = 403
            };
        }
    }
}

/// <summary>
/// Attribute to require a specific feature to be enabled.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireFeatureAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _moduleCode;
    private readonly string _featureName;

    public RequireFeatureAttribute(string moduleCode, string featureName)
    {
        _moduleCode = moduleCode;
        _featureName = featureName;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var moduleManager = context.HttpContext.RequestServices.GetRequiredService<IModuleManager>();

        if (!moduleManager.IsFeatureEnabled(_moduleCode, _featureName))
        {
            context.Result = new ObjectResult(new
            {
                Success = false,
                Error = $"Feature '{_featureName}' in module '{_moduleCode}' is not available."
            })
            {
                StatusCode = 403
            };
        }
    }
}
