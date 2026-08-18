using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

using ERP.API.Controllers.Base;
using ERP.Application.Common.Commands.Organizations;
using ERP.Application.Common.Queries.Organizations;
using ERP.Application.Common.Modules;

namespace ERP.API.Controllers.Common;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/organizations/{organizationId}/modules")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class OrganizationModulesController : BaseApiController
{
    private readonly IMediator _mediator;

    public OrganizationModulesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all modules for an organization
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetModules(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var query = new GetOrganizationModulesQuery
        {
            OrganizationId = organizationId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Enable a module for an organization
    /// </summary>
    [HttpPost("{moduleCode}/enable")]
    public async Task<IActionResult> EnableModule(
        Guid organizationId,
        string moduleCode,
        CancellationToken cancellationToken)
    {
        var command = new EnableOrganizationModuleCommand
        {
            OrganizationId = organizationId,
            ModuleCode = moduleCode
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Disable a module for an organization
    /// </summary>
    [HttpDelete("{moduleCode}/disable")]
    public async Task<IActionResult> DisableModule(
        Guid organizationId,
        string moduleCode,
        CancellationToken cancellationToken)
    {
        var command = new DisableOrganizationModuleCommand
        {
            OrganizationId = organizationId,
            ModuleCode = moduleCode
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all available modules (configuration)
    /// </summary>
    [HttpGet("/api/v1/modules/config")]
    public IActionResult GetModuleConfiguration()
    {
        var modules = ModuleConfigurationLoader.GetAllModules()
            .Select(m => new
            {
                m.Code,
                m.Module,
                Tier = m.Tier,
                Features = m.Features.Select(f => new
                {
                    f.Key,
                    f.Value.Description
                })
            })
            .ToList();

        return Success(new { Modules = modules });
    }

    /// <summary>
    /// Get all available permissions by module
    /// </summary>
    [HttpGet("/api/v1/modules/permissions")]
    public IActionResult GetModulePermissions()
    {
        var permissions = PermissionDefinitions.GetModulePermissions();
        return Success(new { Modules = permissions });
    }
}
