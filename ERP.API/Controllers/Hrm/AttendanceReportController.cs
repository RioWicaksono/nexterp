using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

using ERP.API.Controllers.Base;
using ERP.Application.Hrm.Queries;

namespace ERP.API.Controllers.Hrm;

/// <summary>
/// Attendance report endpoints for HRM Dashboard
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/reports")]
[Authorize]
public class AttendanceReportController : BaseApiController
{
    private readonly IMediator _mediator;

    public AttendanceReportController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get attendance report with summary
    /// </summary>
    [HttpGet("attendance")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAttendanceReport(
        [FromQuery] Guid? departmentId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAttendanceReportQuery
        {
            DepartmentId = departmentId,
            StartDate = startDate ?? DateTime.UtcNow.AddDays(-30),
            EndDate = endDate ?? DateTime.UtcNow
        };

        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
}
