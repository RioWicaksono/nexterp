using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

using ERP.API.Controllers.Base;
using ERP.Application.Hrm.Commands.Attendances;
using ERP.Application.Hrm.DTOs;
using ERP.Application.Hrm.Queries;

namespace ERP.API.Controllers.Hrm;

/// <summary>
/// Attendance management endpoints
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/attendances")]
[Authorize]
public class AttendancesController : BaseApiController
{
    private readonly IMediator _mediator;

    public AttendancesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all attendance records with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AttendanceDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAttendances(
        [FromQuery] Guid? employeeId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAttendancesQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get attendance by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AttendanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAttendanceByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Record attendance
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecordAttendance([FromBody] RecordAttendanceDto request, CancellationToken cancellationToken)
    {
        var command = new RecordAttendanceCommand
        {
            EmployeeId = request.EmployeeId,
            Date = request.Date,
            Status = request.Status,
            CheckInTime = request.CheckInTime,
            CheckOutTime = request.CheckOutTime,
            Notes = request.Notes
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Created($"api/v1/attendances/{result.Value}", result);

        return HandleResult(result);
    }

    /// <summary>
    /// Check in
    /// </summary>
    [HttpPost("check-in")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckIn([FromBody] CheckInDto request, CancellationToken cancellationToken)
    {
        var command = new CheckInCommand
        {
            EmployeeId = request.EmployeeId,
            CheckInTime = request.CheckInTime ?? DateTime.UtcNow,
            Location = request.Location
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Check out
    /// </summary>
    [HttpPost("check-out")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutDto request, CancellationToken cancellationToken)
    {
        var command = new CheckOutCommand
        {
            EmployeeId = request.EmployeeId,
            CheckOutTime = request.CheckOutTime ?? DateTime.UtcNow
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get my attendance (current user)
    /// </summary>
    [HttpGet("my-attendance")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AttendanceDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyAttendance(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement query for current user
        return Error("Not implemented yet", StatusCodes.Status501NotImplemented);
    }
}

/// <summary>
/// DTO for check in
/// </summary>
public class CheckInDto
{
    public Guid EmployeeId { get; set; }
    public DateTime? CheckInTime { get; set; }
    public string? Location { get; set; }
}

/// <summary>
/// DTO for check out
/// </summary>
public class CheckOutDto
{
    public Guid EmployeeId { get; set; }
    public DateTime? CheckOutTime { get; set; }
}
