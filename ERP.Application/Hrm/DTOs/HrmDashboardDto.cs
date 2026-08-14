using ERP.Domain.Hrm.Enums;

namespace ERP.Application.Hrm.DTOs;

/// <summary>
/// HRM Dashboard summary DTO.
/// </summary>
public record HrmDashboardDto
{
    public EmployeeOverviewDto Employees { get; init; } = new();
    public AttendanceOverviewDto Attendance { get; init; } = new();
    public LeaveOverviewDto Leave { get; init; } = new();
    public PayrollOverviewDto Payroll { get; init; } = new();
    public List<RecentActivityDto> RecentActivities { get; init; } = new();
    public List<UpcomingEventDto> UpcomingEvents { get; init; } = new();
}

/// <summary>
/// Employee statistics overview.
/// </summary>
public record EmployeeOverviewDto
{
    public int TotalEmployees { get; init; }
    public int ActiveEmployees { get; init; }
    public int NewHiresThisMonth { get; init; }
    public int TerminatedThisMonth { get; init; }
    public int OnLeave { get; init; }
    public int Contractors { get; init; }
    public Dictionary<string, int> ByDepartment { get; init; } = new();
    public Dictionary<string, int> ByPosition { get; init; } = new();
}

/// <summary>
/// Attendance statistics overview.
/// </summary>
public record AttendanceOverviewDto
{
    public int TotalPresentToday { get; init; }
    public int TotalAbsentToday { get; init; }
    public int TotalLateToday { get; init; }
    public decimal AverageWorkHours { get; init; }
    public int PendingOvertimeRequests { get; init; }
    public int ApprovedOvertimeRequests { get; init; }
    public int TotalOvertimeHoursThisMonth { get; init; }
}

/// <summary>
/// Leave statistics overview.
/// </summary>
public record LeaveOverviewDto
{
    public int PendingRequests { get; init; }
    public int ApprovedThisMonth { get; init; }
    public int RejectedThisMonth { get; init; }
    public int EmployeesOnLeaveToday { get; init; }
    public Dictionary<string, decimal> LeaveBalanceSummary { get; init; } = new();
}

/// <summary>
/// Payroll statistics overview.
/// </summary>
public record PayrollOverviewDto
{
    public PayrollStatus LastPayrollStatus { get; init; }
    public decimal LastPayrollTotal { get; init; }
    public int EmployeesPaidThisMonth { get; init; }
    public int PendingPayrolls { get; init; }
    public decimal TotalThrPaidThisYear { get; init; }
    public decimal TotalPPh21Collected { get; init; }
}

/// <summary>
/// Recent activity item.
/// </summary>
public record RecentActivityDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string EmployeeName { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public string Icon { get; init; } = string.Empty;
}

/// <summary>
/// Upcoming event item.
/// </summary>
public record UpcomingEventDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public string? Description { get; init; }
}

/// <summary>
/// Daily attendance report.
/// </summary>
public record DailyAttendanceReportDto
{
    public DateTime Date { get; init; }
    public int TotalEmployees { get; init; }
    public int Present { get; init; }
    public int Absent { get; init; }
    public int Late { get; init; }
    public int OnLeave { get; init; }
    public int Remote { get; init; }
    public decimal AttendanceRate { get; init; }
    public List<EmployeeAttendanceDto> EmployeeAttendances { get; init; } = new();
}

/// <summary>
/// Employee attendance detail.
/// </summary>
public record EmployeeAttendanceDto
{
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime? CheckIn { get; init; }
    public DateTime? CheckOut { get; init; }
    public decimal WorkHours { get; init; }
    public bool IsLate { get; init; }
}

/// <summary>
/// Department statistics.
/// </summary>
public record DepartmentStatsDto
{
    public Guid DepartmentId { get; init; }
    public string DepartmentName { get; init; } = string.Empty;
    public int EmployeeCount { get; init; }
    public int ActiveEmployees { get; init; }
    public decimal AverageSalary { get; init; }
    public int OpenPositions { get; init; }
    public int PendingLeaveRequests { get; init; }
    public decimal AttendanceRate { get; init; }
}
