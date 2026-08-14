using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.API.Controllers.Base;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using MediatR;
using Asp.Versioning;

namespace ERP.API.Controllers.Mobile;

/// <summary>
/// Mobile API endpoints for employee self-service (clock-in/out, leave requests, etc.)
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/mobile")]
[Authorize]
public class MobileApiController : BaseApiController
{
    private readonly ICurrentUserService _currentUser;
    private readonly IMediator _mediator;

    public MobileApiController(ICurrentUserService currentUser, IMediator mediator)
    {
        _currentUser = currentUser;
        _mediator = mediator;
    }

    /// <summary>
    /// Clock in from mobile device.
    /// </summary>
    [HttpPost("attendance/clock-in")]
    public async Task<IActionResult> ClockIn([FromBody] MobileClockInRequest request)
    {
        var employeeId = await GetCurrentEmployeeIdAsync();
        if (employeeId == Guid.Empty)
            return Error("Employee not found", 404);

        // TODO: Implement actual clock-in command via MediatR
        return Success(new
        {
            Success = true,
            Message = "Clock-in recorded",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Clock out from mobile device.
    /// </summary>
    [HttpPost("attendance/clock-out")]
    public async Task<IActionResult> ClockOut([FromBody] MobileClockOutRequest request)
    {
        var employeeId = await GetCurrentEmployeeIdAsync();
        if (employeeId == Guid.Empty)
            return Error("Employee not found", 404);

        // TODO: Implement actual clock-out command via MediatR
        return Success(new
        {
            Success = true,
            Message = "Clock-out recorded",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get today's attendance status.
    /// </summary>
    [HttpGet("attendance/today")]
    public async Task<IActionResult> GetTodayAttendance()
    {
        var employeeId = await GetCurrentEmployeeIdAsync();
        if (employeeId == Guid.Empty)
            return Error("Employee not found", 404);

        // TODO: Implement actual query via MediatR
        return Success(new
        {
            Date = DateTime.UtcNow.Date,
            Status = "NOT_CHECKED_IN",
            CheckInTime = (DateTime?)null,
            CheckOutTime = (DateTime?)null,
            WorkingHours = (decimal?)null
        });
    }

    /// <summary>
    /// Get attendance history.
    /// </summary>
    [HttpGet("attendance/history")]
    public async Task<IActionResult> GetAttendanceHistory(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var employeeId = await GetCurrentEmployeeIdAsync();
        if (employeeId == Guid.Empty)
            return Error("Employee not found", 404);

        // TODO: Implement actual query via MediatR
        return Success(new
        {
            StartDate = startDate,
            EndDate = endDate,
            Records = Array.Empty<object>()
        });
    }

    /// <summary>
    /// Get leave balance.
    /// </summary>
    [HttpGet("leave/balance")]
    public async Task<IActionResult> GetLeaveBalance()
    {
        var employeeId = await GetCurrentEmployeeIdAsync();
        if (employeeId == Guid.Empty)
            return Error("Employee not found", 404);

        // TODO: Implement actual query via MediatR
        return Success(new
        {
            Year = DateTime.UtcNow.Year,
            Balances = Array.Empty<object>()
        });
    }

    /// <summary>
    /// Get payslip summary.
    /// </summary>
    [HttpGet("payroll/payslip")]
    public async Task<IActionResult> GetPayslipSummary(
        [FromQuery] int? year = null,
        [FromQuery] int? month = null)
    {
        var employeeId = await GetCurrentEmployeeIdAsync();
        if (employeeId == Guid.Empty)
            return Error("Employee not found", 404);

        var y = year ?? DateTime.UtcNow.Year;
        var m = month ?? DateTime.UtcNow.Month;

        // TODO: Implement actual query via MediatR
        return Success(new
        {
            Year = y,
            Month = m,
            BasicSalary = 0m,
            TotalEarnings = 0m,
            TotalDeductions = 0m,
            NetSalary = 0m
        });
    }

    /// <summary>
    /// Get pending approvals count.
    /// </summary>
    [HttpGet("approvals/pending/count")]
    public async Task<IActionResult> GetPendingApprovalsCount()
    {
        var userId = _currentUser.UserId;
        if (userId == null)
            return Error("User not found", 404);

        // TODO: Implement actual query via MediatR
        return Success(new { Count = 0 });
    }

    private async Task<Guid> GetCurrentEmployeeIdAsync()
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Guid.Empty;

        // TODO: Query employee by user ID
        return Guid.Empty;
    }
}

/// <summary>
/// Mobile clock in request.
/// </summary>
public record MobileClockInRequest(
    string? Location,
    string? DeviceId);

/// <summary>
/// Mobile clock out request.
/// </summary>
public record MobileClockOutRequest(
    string? Location);
