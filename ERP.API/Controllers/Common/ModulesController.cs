using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.API.Controllers.Base;
using ERP.Application.Common.Modules;
using ERP.Application.Common.Interfaces;
using Asp.Versioning;

namespace ERP.API.Controllers.Common;

/// <summary>
/// Module management endpoints for administrators.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/modules")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class ModulesController : BaseApiController
{
    private readonly IModuleManager _moduleManager;

    public ModulesController(IModuleManager moduleManager)
    {
        _moduleManager = moduleManager;
    }

    /// <summary>
    /// Get all available modules with their status.
    /// </summary>
    [HttpGet]
    public IActionResult GetAllModules()
    {
        var modules = ModuleConfigurationLoader.GetAllModules();
        return Success(new
        {
            Modules = modules.Select(m => new
            {
                m.Module,
                m.Code,
                m.Enabled,
                m.Tier,
                Features = m.Features.Select(f => new
                {
                    Name = f.Key,
                    f.Value.Enabled,
                    f.Value.Description
                }),
                m.Settings
            }),
            Total = modules.Count,
            Enabled = modules.Count(m => m.Enabled)
        });
    }

    /// <summary>
    /// Get module configuration by code.
    /// </summary>
    [HttpGet("{code}")]
    public IActionResult GetModule(string code)
    {
        var config = _moduleManager.GetModuleConfig(code);
        if (config.Module == null)
            return NotFoundError($"Module '{code}' not found");

        return Success(config);
    }

    /// <summary>
    /// Check if a module is enabled.
    /// </summary>
    [HttpGet("{code}/status")]
    public IActionResult GetModuleStatus(string code)
    {
        var isEnabled = _moduleManager.IsModuleEnabled(code);
        return Success(new
        {
            Code = code,
            Enabled = isEnabled
        });
    }

    /// <summary>
    /// Check if a feature is enabled within a module.
    /// </summary>
    [HttpGet("{code}/features/{featureName}")]
    public IActionResult GetFeatureStatus(string code, string featureName)
    {
        var isEnabled = _moduleManager.IsFeatureEnabled(code, featureName);
        return Success(new
        {
            Module = code,
            Feature = featureName,
            Enabled = isEnabled
        });
    }

    /// <summary>
    /// Get modules by tier.
    /// </summary>
    [HttpGet("tier/{tier}")]
    public IActionResult GetModulesByTier(string tier)
    {
        var modules = _moduleManager.GetEnabledModules(tier);
        return Success(new
        {
            Tier = tier,
            Modules = modules
        });
    }

    /// <summary>
    /// Get all available tiers.
    /// </summary>
    [HttpGet("tiers")]
    public IActionResult GetAllTiers()
    {
        var manifest = ModuleConfigurationLoader.LoadManifest();
        return Success(new
        {
            Tiers = manifest.Tiers.Select(t => new
            {
                t.Key,
                Name = t.Value.Name,
                Description = t.Value.Description,
                ModuleCount = t.Value.Modules.Count
            })
        });
    }

    /// <summary>
    /// Check module access for current organization.
    /// </summary>
    [HttpGet("{code}/access")]
    public async Task<IActionResult> CheckAccess(string code)
    {
        var organizationId = _currentUser.OrganizationId ?? Guid.Empty;
        if (organizationId == Guid.Empty)
            return Error("Organization not found", 401);

        var hasAccess = await _moduleManager.HasModuleAccessAsync(organizationId, code);
        return Success(new
        {
            Code = code,
            HasAccess = hasAccess
        });
    }

    private ICurrentUserService _currentUser => HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();
}
