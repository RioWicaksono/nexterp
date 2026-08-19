using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

using ERP.API.Controllers.Base;
using ERP.Application.Common.Base;
using ERP.Application.Hrm.Commands.Departments;
using ERP.Application.Common.Queries.Departments;

namespace ERP.API.Controllers.Hrm;

/// <summary>
/// Department management endpoints
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/departments")]
[Authorize]
public class DepartmentsController : BaseApiController
{
    private readonly IMediator _mediator;

    public DepartmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all departments with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDepartmentsQuery
        {
            Page = page,
            PageSize = pageSize,
            Search = search
        };

        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new department
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentDto request, CancellationToken cancellationToken)
    {
        var command = new CreateDepartmentCommand
        {
            OrganizationId = request.OrganizationId,
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            ParentDepartmentId = request.ParentDepartmentId
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Created($"api/v1/departments/{result.Value}", result);

        return HandleResult(result);
    }

    /// <summary>
    /// Update department
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentDto request, CancellationToken cancellationToken)
    {
        var command = new UpdateDepartmentCommand
        {
            DepartmentId = id,
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            ParentDepartmentId = request.ParentDepartmentId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}

/// <summary>
/// DTO for creating department
/// </summary>
public class CreateDepartmentDto
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
}

/// <summary>
/// DTO for updating department
/// </summary>
public class UpdateDepartmentDto
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
}

/// <summary>
/// Position management endpoints
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/positions")]
[Authorize]
public class PositionsController : BaseApiController
{
    private readonly IMediator _mediator;

    public PositionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all positions (simplified - returns empty list for now)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPositions(CancellationToken cancellationToken = default)
    {
        // Return empty list for now - positions CRUD can be added later
        var result = Result<object>.Success(new { items = new object[] { }, totalCount = 0, page = 1, pageSize = 50 });
        return Ok(result);
    }

    /// <summary>
    /// Create a new position
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePositionDto request, CancellationToken cancellationToken)
    {
        var command = new CreatePositionCommand
        {
            OrganizationId = request.OrganizationId,
            DepartmentId = request.DepartmentId,
            Title = request.Title,
            Description = request.Description,
            Grade = request.Grade,
            MinSalary = request.MinSalary,
            MaxSalary = request.MaxSalary
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Created($"api/v1/positions/{result.Value}", result);

        return HandleResult(result);
    }

    /// <summary>
    /// Update position
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePositionDto request, CancellationToken cancellationToken)
    {
        var command = new UpdatePositionCommand
        {
            PositionId = id,
            Title = request.Title,
            Description = request.Description,
            Grade = request.Grade,
            MinSalary = request.MinSalary,
            MaxSalary = request.MaxSalary
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}

/// <summary>
/// DTO for creating position
/// </summary>
public class CreatePositionDto
{
    public Guid OrganizationId { get; set; }
    public Guid DepartmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Grade { get; set; } = 1;
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
}

/// <summary>
/// DTO for updating position
/// </summary>
public class UpdatePositionDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? Grade { get; set; }
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
}
