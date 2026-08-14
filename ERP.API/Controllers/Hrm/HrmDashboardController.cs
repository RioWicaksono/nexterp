using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.API.Controllers.Base;
using ERP.Application.Common.Behaviors;
using ERP.Application.Hrm.DTOs;
using ERP.Application.Hrm.Queries.Hrm;
using Asp.Versioning;

namespace ERP.API.Controllers.Hrm;

/// <summary>
/// HRM Dashboard endpoints.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/hrm/dashboard")]
[Authorize]
[RequiresModule("HRM")]
public class HrmDashboardController : BaseApiController
{
    private readonly IMediator _mediator;

    public HrmDashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get HRM Dashboard data.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] Guid organizationId,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null)
    {
        var query = new GetHrmDashboardQuery(organizationId, year, month);
        var result = await _mediator.Send(query);
        return Success(result);
    }

    /// <summary>
    /// Get daily attendance report.
    /// </summary>
    [HttpGet("attendance/daily")]
    public async Task<IActionResult> GetDailyAttendanceReport(
        [FromQuery] Guid organizationId,
        [FromQuery] DateTime date)
    {
        var query = new GetDailyAttendanceReportQuery(organizationId, date);
        var result = await _mediator.Send(query);
        return Success(result);
    }

    /// <summary>
    /// Get department statistics.
    /// </summary>
    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartmentStats(
        [FromQuery] Guid organizationId,
        [FromQuery] Guid? departmentId = null)
    {
        var query = new GetDepartmentStatsQuery(organizationId, departmentId);
        var result = await _mediator.Send(query);
        return Success(result);
    }

    /// <summary>
    /// Get employee overview.
    /// </summary>
    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployeeOverview(
        [FromQuery] Guid organizationId,
        [FromQuery] Guid? departmentId = null)
    {
        var query = new GetEmployeeOverviewQuery(organizationId, departmentId);
        var result = await _mediator.Send(query);
        return Success(result);
    }

    /// <summary>
    /// Get attendance summary for period.
    /// </summary>
    [HttpGet("attendance/summary")]
    public async Task<IActionResult> GetAttendanceSummary(
        [FromQuery] Guid organizationId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var query = new GetAttendanceSummaryQuery(organizationId, startDate, endDate);
        var result = await _mediator.Send(query);
        return Success(result);
    }
}
