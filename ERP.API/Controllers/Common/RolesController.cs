using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

using ERP.API.Controllers.Base;
using ERP.Application.Base.Commands.Roles;
using ERP.Application.Base.Queries.Roles;
using ERP.Application.Common.DTOs;

namespace ERP.API.Controllers.Common;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/roles")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class RolesController : BaseApiController
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all roles with pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRoles(
        [FromQuery] Guid organizationId,
        [FromQuery] bool? isActive,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRolesQuery
        {
            OrganizationId = organizationId,
            IsActive = isActive,
            Search = search,
            Pagination = new PaginationParams { Page = page, PageSize = pageSize }
        };

        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get role by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromQuery] Guid organizationId,
        CancellationToken cancellationToken)
    {
        var query = new GetRoleByIdQuery
        {
            Id = id,
            OrganizationId = organizationId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateRoleCommand
        {
            OrganizationId = request.OrganizationId,
            Name = request.Name,
            Description = request.Description,
            Permissions = request.Permissions
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Created($"api/v1/roles/{result.Value}", result);

        return HandleResult(result);
    }

    /// <summary>
    /// Update an existing role
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateRoleCommand
        {
            Id = id,
            OrganizationId = request.OrganizationId,
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete a role
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromQuery] Guid organizationId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteRoleCommand
        {
            Id = id,
            OrganizationId = organizationId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Add permissions to a role
    /// </summary>
    [HttpPost("{id:guid}/permissions")]
    public async Task<IActionResult> AddPermissions(
        Guid id,
        [FromBody] AddRolePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddRolePermissionsCommand
        {
            RoleId = id,
            OrganizationId = request.OrganizationId,
            Permissions = request.Permissions
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Remove permissions from a role
    /// </summary>
    [HttpDelete("{id:guid}/permissions")]
    public async Task<IActionResult> RemovePermissions(
        Guid id,
        [FromBody] RemoveRolePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RemoveRolePermissionsCommand
        {
            RoleId = id,
            OrganizationId = request.OrganizationId,
            Permissions = request.Permissions
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all available permissions
    /// </summary>
    [HttpGet("permissions")]
    public IActionResult GetAllPermissions()
    {
        var permissions = PermissionDefinitions.GetAvailablePermissions();
        return Success(new { Permissions = permissions });
    }
}

#region DTOs

public class CreateRoleRequest
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string>? Permissions { get; set; }
}

public class UpdateRoleRequest
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class AddRolePermissionsRequest
{
    public Guid OrganizationId { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public class RemoveRolePermissionsRequest
{
    public Guid OrganizationId { get; set; }
    public List<string> Permissions { get; set; } = new();
}

#endregion

/// <summary>
/// Helper class to get available permissions
/// </summary>
public static class PermissionDefinitions
{
    public static readonly Dictionary<string, List<string>> ModulePermissions = new()
    {
        ["HRM"] = new List<string>
        {
            "hrm.employees.read",
            "hrm.employees.create",
            "hrm.employees.update",
            "hrm.employees.delete",
            "hrm.departments.read",
            "hrm.departments.create",
            "hrm.departments.update",
            "hrm.departments.delete",
            "hrm.attendances.read",
            "hrm.attendances.create",
            "hrm.attendances.update",
            "hrm.leave.read",
            "hrm.leave.approve",
            "hrm.payroll.read",
            "hrm.payroll.process",
            "hrm.reports.read"
        },
        ["INVENTORY"] = new List<string>
        {
            "inventory.items.read",
            "inventory.items.create",
            "inventory.items.update",
            "inventory.items.delete",
            "inventory.stock.read",
            "inventory.stock.adjust",
            "inventory.warehouses.read",
            "inventory.warehouses.manage",
            "inventory.reports.read"
        },
        ["SALES"] = new List<string>
        {
            "sales.orders.read",
            "sales.orders.create",
            "sales.orders.update",
            "sales.orders.delete",
            "sales.invoices.read",
            "sales.invoices.create",
            "sales.invoices.update",
            "sales.customers.read",
            "sales.customers.manage",
            "sales.reports.read"
        },
        ["PURCHASING"] = new List<string>
        {
            "purchasing.orders.read",
            "purchasing.orders.create",
            "purchasing.orders.update",
            "purchasing.orders.delete",
            "purchasing.suppliers.read",
            "purchasing.suppliers.manage",
            "purchasing.reports.read"
        },
        ["ACCOUNTING"] = new List<string>
        {
            "accounting.accounts.read",
            "accounting.accounts.create",
            "accounting.accounts.update",
            "accounting.journals.read",
            "accounting.journals.create",
            "accounting.journals.post",
            "accounting.reports.read",
            "accounting.reports.financial"
        },
        ["PROJECTS"] = new List<string>
        {
            "projects.read",
            "projects.create",
            "projects.update",
            "projects.delete",
            "projects.tasks.read",
            "projects.tasks.manage",
            "projects.reports.read"
        },
        ["ASSETS"] = new List<string>
        {
            "assets.read",
            "assets.create",
            "assets.update",
            "assets.delete",
            "assets.maintenance.read",
            "assets.maintenance.schedule",
            "assets.depreciation.read"
        },
        ["QUALITY"] = new List<string>
        {
            "quality.inspections.read",
            "quality.inspections.create",
            "quality.inspections.update",
            "quality.nc.read",
            "quality.nc.create",
            "quality.nc.resolve"
        },
        ["ANALYTICS"] = new List<string>
        {
            "analytics.dashboard.read",
            "analytics.reports.read",
            "analytics.exports.read",
            "analytics.custom.read"
        },
        ["ADMIN"] = new List<string>
        {
            "admin.users.read",
            "admin.users.create",
            "admin.users.update",
            "admin.users.delete",
            "admin.roles.read",
            "admin.roles.create",
            "admin.roles.update",
            "admin.roles.delete",
            "admin.modules.read",
            "admin.modules.manage",
            "admin.settings.read",
            "admin.settings.update",
            "admin.audit.read"
        }
    };

    public static List<string> GetAvailablePermissions()
    {
        return ModulePermissions.Values.SelectMany(p => p).ToList();
    }

    public static List<ModulePermissionInfo> GetModulePermissions()
    {
        return ModulePermissions.Select(kvp => new ModulePermissionInfo
        {
            Module = kvp.Key,
            Permissions = kvp.Value
        }).ToList();
    }
}

public class ModulePermissionInfo
{
    public string Module { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}
