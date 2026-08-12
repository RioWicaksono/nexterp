using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.API.Controllers.Base;
using ERP.Application.Hrm.Commands.Employees;
using ERP.Application.Hrm.DTOs;
using ERP.Application.Hrm.Queries;

namespace ERP.API.Controllers.Hrm;

/// <summary>
/// Employee management endpoints
/// </summary>
[ApiController]
[Route("api/v1/employees")]
[Authorize]
public class EmployeesController : BaseApiController
{
    private readonly IMediator _mediator;

    public EmployeesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all employees with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EmployeeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployees(
        [FromQuery] Guid? departmentId,
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetEmployeesQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get employee by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetEmployeeByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new employee
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDto request, CancellationToken cancellationToken)
    {
        // Get organization ID from user's claims
        var orgIdClaim = User.FindFirst("org")?.Value;
        if (string.IsNullOrEmpty(orgIdClaim) || !Guid.TryParse(orgIdClaim, out var orgId))
        {
            return Error("Invalid organization", StatusCodes.Status400BadRequest);
        }

        var command = new CreateEmployeeCommand
        {
            UserId = request.UserId,
            OrganizationId = orgId,
            EmployeeNumber = request.EmployeeNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            MaritalStatus = request.MaritalStatus,
            DepartmentId = request.DepartmentId,
            PositionId = request.PositionId,
            EmploymentType = request.EmploymentType,
            HireDate = request.HireDate,
            PersonalEmail = request.PersonalEmail,
            Phone = request.Phone,
            Mobile = request.Mobile
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Created($"api/v1/employees/{result.Value}", result);

        return HandleResult(result);
    }

    /// <summary>
    /// Update employee
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateEmployeeDto request, CancellationToken cancellationToken)
    {
        // TODO: Implement update command
        return Error("Not implemented yet", StatusCodes.Status501NotImplemented);
    }

    /// <summary>
    /// Terminate employee
    /// </summary>
    [HttpPost("{id:guid}/terminate")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Terminate(Guid id, [FromBody] TerminateEmployeeDto request, CancellationToken cancellationToken)
    {
        // TODO: Implement terminate command
        return Error("Not implemented yet", StatusCodes.Status501NotImplemented);
    }
}

/// <summary>
/// DTO for terminating employee
/// </summary>
public class TerminateEmployeeDto
{
    public DateTime TerminationDate { get; set; }
    public string? Reason { get; set; }
}
