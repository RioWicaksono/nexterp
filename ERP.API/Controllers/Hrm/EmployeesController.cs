using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

using ERP.API.Controllers.Base;
using ERP.Application.Hrm.Commands.Employees;
using ERP.Application.Hrm.DTOs;
using ERP.Application.Hrm.Queries;

namespace ERP.API.Controllers.Hrm;

/// <summary>
/// Employee management endpoints
/// </summary>
[ApiVersion("1.0")]
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
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployees(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetEmployeesPaginatedQuery
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            Status = status,
            DepartmentId = departmentId,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get employee by ID with full details
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetEmployeeDetailsQuery { EmployeeId = id };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get employee leave balance summary
    /// </summary>
    [HttpGet("{id:guid}/leave-balance")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaveBalance(Guid id, [FromQuery] int? year = null, CancellationToken cancellationToken = default)
    {
        var query = new GetLeaveBalanceSummaryQuery
        {
            EmployeeId = id,
            Year = year
        };

        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new employee
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDto request, CancellationToken cancellationToken)
    {
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
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeDto request, CancellationToken cancellationToken)
    {
        var command = new UpdateEmployeeCommand
        {
            EmployeeId = id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            MaritalStatus = request.MaritalStatus,
            PersonalEmail = request.PersonalEmail,
            Phone = request.Phone,
            Mobile = request.Mobile,
            DepartmentId = request.DepartmentId,
            PositionId = request.PositionId,
            EmploymentType = request.EmploymentType,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            EmergencyContactRelation = request.EmergencyContactRelation,
            Address = request.Address,
            City = request.City,
            Country = request.Country,
            PostalCode = request.PostalCode,
            BankName = request.BankName,
            BankAccountNumber = request.BankAccountNumber,
            BankAccountName = request.BankAccountName,
            TaxId = request.TaxId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Update employee status
    /// </summary>
    [HttpPost("{id:guid}/status")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateEmployeeStatusDto request, CancellationToken cancellationToken)
    {
        var command = new UpdateEmployeeStatusCommand
        {
            EmployeeId = id,
            Status = request.Status,
            Reason = request.Reason,
            EffectiveDate = request.EffectiveDate
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete (soft delete) employee
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] string? reason = null, CancellationToken cancellationToken = default)
    {
        var command = new DeleteEmployeeCommand
        {
            EmployeeId = id,
            Reason = reason
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}

/// <summary>
/// DTO for updating employee
/// </summary>
public class UpdateEmployeeDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? MaritalStatus { get; set; }
    public string? PersonalEmail { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? PositionId { get; set; }
    public string? EmploymentType { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankAccountName { get; set; }
    public string? TaxId { get; set; }
}

/// <summary>
/// DTO for updating employee status
/// </summary>
public class UpdateEmployeeStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime? EffectiveDate { get; set; }
}
