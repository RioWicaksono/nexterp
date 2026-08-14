using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

using ERP.API.Controllers.Base;
using ERP.Application.Hrm.Commands.Overtimes;

namespace ERP.API.Controllers.Hrm;

/// <summary>
/// Overtime request management endpoints
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/overtime-requests")]
[Authorize]
public class OvertimeRequestsController : BaseApiController
{
    private readonly IMediator _mediator;

    public OvertimeRequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new overtime request
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOvertimeRequestDto request, CancellationToken cancellationToken)
    {
        var command = new CreateOvertimeRequestCommand
        {
            EmployeeId = request.EmployeeId,
            WorkDate = request.WorkDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            OvertimeType = request.OvertimeType,
            Reason = request.Reason
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Created($"api/v1/overtime-requests/{result.Value}", result);

        return HandleResult(result);
    }

    /// <summary>
    /// Approve overtime request
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveOvertimeDto request, CancellationToken cancellationToken)
    {
        var command = new ApproveOvertimeRequestCommand
        {
            OvertimeRequestId = id,
            ApproverId = request.ApproverId,
            Approved = true,
            ApprovedHours = request.ApprovedHours,
            Notes = request.Notes
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Reject overtime request
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectOvertimeDto request, CancellationToken cancellationToken)
    {
        var command = new ApproveOvertimeRequestCommand
        {
            OvertimeRequestId = id,
            ApproverId = request.ApproverId,
            Approved = false,
            Notes = request.Reason
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Cancel overtime request
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var command = new CancelOvertimeRequestCommand
        {
            OvertimeRequestId = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}

/// <summary>
/// DTO for creating overtime request
/// </summary>
public class CreateOvertimeRequestDto
{
    public Guid EmployeeId { get; set; }
    public DateTime WorkDate { get; set; }
    public string StartTime { get; set; } = string.Empty; // Format: "HH:mm"
    public string EndTime { get; set; } = string.Empty;   // Format: "HH:mm"
    public string OvertimeType { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

/// <summary>
/// DTO for approving overtime request
/// </summary>
public class ApproveOvertimeDto
{
    public Guid ApproverId { get; set; }
    public decimal? ApprovedHours { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for rejecting overtime request
/// </summary>
public class RejectOvertimeDto
{
    public Guid ApproverId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
