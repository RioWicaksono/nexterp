using ERP.Application.Common.Interfaces;
using ERP.Application.Hrm.DTOs;
using ERP.Domain.Hrm.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Hrm.Queries.Hrm;

/// <summary>
/// Handler for HRM Dashboard queries.
/// </summary>
public class DashboardHandler :
    IRequestHandler<GetHrmDashboardQuery, HrmDashboardDto>,
    IRequestHandler<GetDailyAttendanceReportQuery, DailyAttendanceReportDto>,
    IRequestHandler<GetDepartmentStatsQuery, List<DepartmentStatsDto>>,
    IRequestHandler<GetEmployeeOverviewQuery, EmployeeOverviewDto>,
    IRequestHandler<GetAttendanceSummaryQuery, AttendanceOverviewDto>
{
    private readonly IApplicationDbContext _context;

    public DashboardHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HrmDashboardDto> Handle(GetHrmDashboardQuery request, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var month = request.Month ?? today.Month;
        var year = request.Year ?? today.Year;
        var startOfMonth = new DateTime(year, month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        // Employee overview
        var employees = await _context.Employees
            .Where(e => e.OrganizationId == request.OrganizationId)
            .ToListAsync(ct);

        var employeeOverview = new EmployeeOverviewDto
        {
            TotalEmployees = employees.Count,
            ActiveEmployees = employees.Count(e => e.Status == EmployeeStatus.Active),
            NewHiresThisMonth = employees.Count(e => e.HireDate >= startOfMonth && e.HireDate <= endOfMonth),
            TerminatedThisMonth = employees.Count(e => e.TerminationDate >= startOfMonth && e.TerminationDate <= endOfMonth),
            OnLeave = employees.Count(e => e.Status == EmployeeStatus.OnLeave),
            Contractors = employees.Count(e => e.EmploymentType == EmploymentType.Contract),
            ByDepartment = employees
                .Where(e => e.Department != null)
                .GroupBy(e => e.Department!.Name)
                .ToDictionary(g => g.Key, g => g.Count()),
            ByPosition = new Dictionary<string, int>()
        };

        // Attendance overview
        var todayAttendances = await _context.Attendances
            .Include(a => a.Employee)
            .Where(a => a.OrganizationId == request.OrganizationId && a.Date.Date == today)
            .ToListAsync(ct);

        var monthAttendances = await _context.Attendances
            .Where(a => a.OrganizationId == request.OrganizationId &&
                       a.Date >= startOfMonth && a.Date <= endOfMonth)
            .ToListAsync(ct);

        var pendingOvertime = await _context.OvertimeRequests
            .Where(o => o.OrganizationId == request.OrganizationId && o.Status == OvertimeStatus.Pending)
            .CountAsync(ct);

        var attendanceOverview = new AttendanceOverviewDto
        {
            TotalPresentToday = todayAttendances.Count(a => a.Status == AttendanceStatus.Present),
            TotalAbsentToday = employees.Count(e => e.Status == EmployeeStatus.Active) - todayAttendances.Count(a => a.Status == AttendanceStatus.Present),
            TotalLateToday = 0,  // Would need Shift info to determine lateness
            AverageWorkHours = monthAttendances.Any()
                ? monthAttendances.Where(a => a.WorkingHours.HasValue).Average(a => (decimal)a.WorkingHours!.Value.TotalHours)
                : 0,
            PendingOvertimeRequests = pendingOvertime,
            ApprovedOvertimeRequests = monthAttendances.Count(a => a.OvertimeHours > 0),
            TotalOvertimeHoursThisMonth = (int)monthAttendances.Sum(a => a.OvertimeHours)
        };

        // Leave overview
        var pendingLeaves = await _context.LeaveRequests
            .Where(l => l.OrganizationId == request.OrganizationId && l.Status == LeaveStatus.Pending)
            .CountAsync(ct);

        var leaveOverview = new LeaveOverviewDto
        {
            PendingRequests = pendingLeaves,
            ApprovedThisMonth = await _context.LeaveRequests
                .Where(l => l.OrganizationId == request.OrganizationId &&
                           l.Status == LeaveStatus.Approved &&
                           l.ApprovedAt >= startOfMonth && l.ApprovedAt <= endOfMonth)
                .CountAsync(ct),
            RejectedThisMonth = await _context.LeaveRequests
                .Where(l => l.OrganizationId == request.OrganizationId &&
                           l.Status == LeaveStatus.Rejected &&
                           l.RejectedAt >= startOfMonth && l.RejectedAt <= endOfMonth)
                .CountAsync(ct),
            EmployeesOnLeaveToday = employees.Count(e => e.Status == EmployeeStatus.OnLeave)
        };

        // Payroll overview
        var lastPayroll = await _context.Payrolls
            .Where(p => p.OrganizationId == request.OrganizationId)
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .FirstOrDefaultAsync(ct);

        var pendingPayrolls = await _context.Payrolls
            .Where(p => p.OrganizationId == request.OrganizationId && p.Status == PayrollStatus.Draft)
            .CountAsync(ct);

        var payrollOverview = new PayrollOverviewDto
        {
            LastPayrollStatus = lastPayroll?.Status ?? PayrollStatus.Draft,
            LastPayrollTotal = lastPayroll?.NetSalary ?? 0,
            EmployeesPaidThisMonth = await _context.Payrolls
                .Where(p => p.OrganizationId == request.OrganizationId &&
                           p.Year == year && p.Month == month &&
                           p.Status == PayrollStatus.PaymentPending)
                .CountAsync(ct),
            PendingPayrolls = pendingPayrolls,
            TotalThrPaidThisYear = await _context.Payrolls
                .Where(p => p.OrganizationId == request.OrganizationId && p.Year == year && p.Thr > 0)
                .SumAsync(p => p.Thr, ct),
            TotalPPh21Collected = await _context.Payrolls
                .Where(p => p.OrganizationId == request.OrganizationId && p.Year == year)
                .SumAsync(p => p.PPh21Deduction, ct)
        };

        return new HrmDashboardDto
        {
            Employees = employeeOverview,
            Attendance = attendanceOverview,
            Leave = leaveOverview,
            Payroll = payrollOverview,
            RecentActivities = new List<RecentActivityDto>(),
            UpcomingEvents = new List<UpcomingEventDto>()
        };
    }

    public async Task<DailyAttendanceReportDto> Handle(GetDailyAttendanceReportQuery request, CancellationToken ct)
    {
        var employees = await _context.Employees
            .Include(e => e.Department)
            .Where(e => e.OrganizationId == request.OrganizationId && e.Status == EmployeeStatus.Active)
            .ToListAsync(ct);

        var attendances = await _context.Attendances
            .Include(a => a.Employee)
                .ThenInclude(e => e!.Department)
            .Where(a => a.OrganizationId == request.OrganizationId && a.Date.Date == request.Date.Date)
            .ToListAsync(ct);

        var attendanceDict = attendances.ToDictionary(a => a.EmployeeId);
        var presentCount = attendances.Count(a => a.Status == AttendanceStatus.Present);
        var lateCount = 0;  // Would need Shift info to determine lateness

        var employeeAttendances = employees.Select(e =>
        {
            attendanceDict.TryGetValue(e.Id, out var attendance);
            return new EmployeeAttendanceDto
            {
                EmployeeId = e.Id,
                EmployeeName = $"{e.FirstName} {e.LastName}".Trim(),
                Department = e.Department?.Name ?? "",
                Status = attendance?.Status.ToString() ?? "Not Recorded",
                CheckIn = attendance?.CheckInTime,
                CheckOut = attendance?.CheckOutTime,
                WorkHours = attendance?.WorkingHours.HasValue == true
                    ? (decimal)attendance.WorkingHours!.Value.TotalHours
                    : 0,
                IsLate = false  // Would need Shift info to determine
            };
        }).ToList();

        return new DailyAttendanceReportDto
        {
            Date = request.Date,
            TotalEmployees = employees.Count,
            Present = presentCount,
            Absent = employees.Count - presentCount,
            Late = lateCount,
            OnLeave = employees.Count(e => e.Status == EmployeeStatus.OnLeave),
            Remote = 0,
            AttendanceRate = employees.Count > 0 ? (decimal)presentCount / employees.Count * 100 : 0,
            EmployeeAttendances = employeeAttendances
        };
    }

    public async Task<List<DepartmentStatsDto>> Handle(GetDepartmentStatsQuery request, CancellationToken ct)
    {
        var departments = await _context.Departments
            .Where(d => d.OrganizationId == request.OrganizationId)
            .Where(d => request.DepartmentId == null || d.Id == request.DepartmentId)
            .Include(d => d.Employees)
            .ToListAsync(ct);

        var stats = new List<DepartmentStatsDto>();

        foreach (var dept in departments)
        {
            var pendingLeaves = await _context.LeaveRequests
                .Where(l => l.Employee!.DepartmentId == dept.Id && l.Status == LeaveStatus.Pending)
                .CountAsync(ct);

            var attendances = await _context.Attendances
                .Where(a => a.Employee!.DepartmentId == dept.Id && a.Date.Date == DateTime.UtcNow.Date)
                .ToListAsync(ct);

            stats.Add(new DepartmentStatsDto
            {
                DepartmentId = dept.Id,
                DepartmentName = dept.Name,
                EmployeeCount = dept.Employees.Count,
                ActiveEmployees = dept.Employees.Count(e => e.Status == EmployeeStatus.Active),
                AverageSalary = dept.Employees.Any() ? dept.Employees.Average(e => e.BasicSalary) : 0,
                OpenPositions = 0,
                PendingLeaveRequests = pendingLeaves,
                AttendanceRate = attendances.Count > 0
                    ? (decimal)attendances.Count(a => a.Status == AttendanceStatus.Present) / attendances.Count * 100
                    : 0
            });
        }

        return stats;
    }

    public async Task<EmployeeOverviewDto> Handle(GetEmployeeOverviewQuery request, CancellationToken ct)
    {
        var employees = await _context.Employees
            .Include(e => e.Department)
            .Where(e => e.OrganizationId == request.OrganizationId)
            .Where(e => request.DepartmentId == null || e.DepartmentId == request.DepartmentId)
            .ToListAsync(ct);

        return new EmployeeOverviewDto
        {
            TotalEmployees = employees.Count,
            ActiveEmployees = employees.Count(e => e.Status == EmployeeStatus.Active),
            NewHiresThisMonth = 0,
            TerminatedThisMonth = 0,
            OnLeave = employees.Count(e => e.Status == EmployeeStatus.OnLeave),
            Contractors = employees.Count(e => e.EmploymentType == EmploymentType.Contract),
            ByDepartment = employees
                .Where(e => e.Department != null)
                .GroupBy(e => e.Department!.Name)
                .ToDictionary(g => g.Key, g => g.Count()),
            ByPosition = new Dictionary<string, int>()
        };
    }

    public async Task<AttendanceOverviewDto> Handle(GetAttendanceSummaryQuery request, CancellationToken ct)
    {
        var attendances = await _context.Attendances
            .Where(a => a.OrganizationId == request.OrganizationId &&
                       a.Date >= request.StartDate && a.Date <= request.EndDate)
            .ToListAsync(ct);

        return new AttendanceOverviewDto
        {
            TotalPresentToday = attendances.Count(a => a.Status == AttendanceStatus.Present),
            TotalAbsentToday = 0,
            TotalLateToday = 0,  // Would need Shift info
            AverageWorkHours = attendances.Any()
                ? attendances.Where(a => a.WorkingHours.HasValue).Average(a => (decimal)a.WorkingHours!.Value.TotalHours)
                : 0,
            PendingOvertimeRequests = 0,
            ApprovedOvertimeRequests = attendances.Count(a => a.OvertimeHours > 0),
            TotalOvertimeHoursThisMonth = (int)attendances.Sum(a => a.OvertimeHours)
        };
    }
}
