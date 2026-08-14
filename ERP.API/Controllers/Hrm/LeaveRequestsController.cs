using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

using ERP.API.Controllers.Base;
using ERP.Application.Hrm.Commands.Leaves;
using ERP.Application.Hrm.DTOs;
using ERP.Application.Hrm.Queries;

namespace ERP.API.Controllers.Hrm;

/// <summary>
/// Leave management endpoints
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/leave-requests")]
[Authorize]
public class LeaveRequestsController : BaseApiController
{
    private readonly IMediator _mediator;

    public LeaveRequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all leave requests
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaveRequests(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLeaveRequestsQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get leave request by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLeaveRequestByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new leave request
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateLeaveRequestDto request, CancellationToken cancellationToken)
    {
        var command = new CreateLeaveRequestCommand
        {
            EmployeeId = request.EmployeeId,
            LeaveType = request.LeaveType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Reason = request.Reason,
            HalfDay = request.HalfDay
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Created($"api/v1/leave-requests/{result.Value}", result);

        return HandleResult(result);
    }

    /// <summary>
    /// Approve leave request
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveLeaveDto request, CancellationToken cancellationToken)
    {
        var command = new ApproveLeaveRequestCommand
        {
            LeaveRequestId = id,
            ApproverId = request.ApproverId,
            Approved = true
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Reject leave request
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectLeaveDto request, CancellationToken cancellationToken)
    {
        var command = new ApproveLeaveRequestCommand
        {
            LeaveRequestId = id,
            ApproverId = request.ApproverId,
            Approved = false,
            Reason = request.Reason
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Cancel leave request
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var command = new CancelLeaveRequestCommand
        {
            LeaveRequestId = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Set leave balance for employee
    /// </summary>
    [HttpPost("leave-balances")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetLeaveBalance([FromBody] SetLeaveBalanceDto request, CancellationToken cancellationToken)
    {
        var command = new SetLeaveBalanceCommand
        {
            EmployeeId = request.EmployeeId,
            LeaveType = request.LeaveType,
            Year = request.Year,
            TotalDays = request.TotalDays,
            CarryForwardDays = request.CarryForwardDays,
            Notes = request.Notes
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Auto-allocate leave balance based on years of service
    /// </summary>
    [HttpPost("{employeeId:guid}/auto-allocate-leave")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AutoAllocateLeave(Guid employeeId, [FromQuery] int? year = null, CancellationToken cancellationToken = default)
    {
        var command = new AutoAllocateLeaveBalanceCommand
        {
            EmployeeId = employeeId,
            Year = year ?? DateTime.UtcNow.Year
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}

/// <summary>
/// DTO for approving leave
/// </summary>
public class ApproveLeaveDto
{
    public Guid ApproverId { get; set; }
}

/// <summary>
/// DTO for rejecting leave
/// </summary>
public class RejectLeaveDto
{
    public Guid ApproverId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// DTO for setting leave balance
/// </summary>
public class SetLeaveBalanceDto
{
    public Guid EmployeeId { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal TotalDays { get; set; }
    public decimal CarryForwardDays { get; set; }
    public string? Notes { get; set; }
}
