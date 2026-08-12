using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.API.Controllers.Base;
using ERP.Application.Hrm.Commands.Leaves;
using ERP.Application.Hrm.DTOs;
using ERP.Application.Hrm.Queries;

namespace ERP.API.Controllers.Hrm;

/// <summary>
/// Leave management endpoints
/// </summary>
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
    /// Get all leave requests with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LeaveRequestDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaveRequests(
        [FromQuery] Guid? employeeId,
        [FromQuery] string? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetLeaveRequestsQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get leave request by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<LeaveRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLeaveRequestByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new leave request
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        // TODO: Implement cancel command
        return Error("Not implemented yet", StatusCodes.Status501NotImplemented);
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
