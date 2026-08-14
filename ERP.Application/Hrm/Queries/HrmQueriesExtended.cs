using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Behaviors;
using ERP.Application.Common.Extensions;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Hrm.Entities;
using ERP.Domain.Hrm.Enums;

namespace ERP.Application.Hrm.Queries;

/// <summary>
/// Query to get paginated employee list
/// </summary>
[RequiresModule("HRM")]
public class GetEmployeesPaginatedQuery : IRequest<Result<PaginatedResult<EmployeeListDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public string? Status { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}

/// <summary>
/// Handler for GetEmployeesPaginatedQuery
/// </summary>
public class GetEmployeesPaginatedHandler : IRequestHandler<GetEmployeesPaginatedQuery, Result<PaginatedResult<EmployeeListDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetEmployeesPaginatedHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<EmployeeListDto>>> Handle(
        GetEmployeesPaginatedQuery request,
        CancellationToken cancellationToken)
    {
        // Normalize pagination parameters
        var (page, pageSize) = PaginationValidator.Normalize(request.Page, request.PageSize);

        var query = _context.Employees
            .AsNoTracking()
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(e =>
                e.EmployeeNumber.ToLower().Contains(search) ||
                e.FirstName.ToLower().Contains(search) ||
                (e.LastName != null && e.LastName.ToLower().Contains(search)));
        }

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<EmployeeStatus>(request.Status, true, out var status))
        {
            query = query.Where(e => e.Status == status);
        }

        // Apply department filter
        if (request.DepartmentId.HasValue)
        {
            query = query.Where(e => e.DepartmentId == request.DepartmentId.Value);
        }

        // Apply sorting
        query = (request.SortBy?.ToLower(), request.SortDescending) switch
        {
            ("name", true) => query.OrderByDescending(e => e.FirstName).ThenByDescending(e => e.LastName),
            ("name", false) => query.OrderBy(e => e.FirstName).ThenBy(e => e.LastName),
            ("hiredate", true) => query.OrderByDescending(e => e.HireDate),
            ("hiredate", false) => query.OrderBy(e => e.HireDate),
            ("status", true) => query.OrderByDescending(e => e.Status),
            ("status", false) => query.OrderBy(e => e.Status),
            _ => query.OrderBy(e => e.EmployeeNumber)
        };

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EmployeeListDto
            {
                Id = e.Id,
                EmployeeNumber = e.EmployeeNumber,
                FullName = e.FullName,
                FirstName = e.FirstName,
                LastName = e.LastName,
                DateOfBirth = e.DateOfBirth,
                Gender = e.Gender.ToString(),
                MaritalStatus = e.MaritalStatus.ToString(),
                Status = e.Status.ToString(),
                DepartmentId = e.DepartmentId,
                PositionId = e.PositionId,
                EmploymentType = e.EmploymentType.ToString(),
                HireDate = e.HireDate,
                YearsOfService = e.YearsOfService,
                Phone = e.Phone,
                Mobile = e.Mobile,
                PersonalEmail = e.PersonalEmail
            })
            .ToListAsync(cancellationToken);

        return Result<PaginatedResult<EmployeeListDto>>.Success(new PaginatedResult<EmployeeListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }
}

/// <summary>
/// Employee list DTO for paginated results
/// </summary>
public class EmployeeListDto
{
    public Guid Id { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string MaritalStatus { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public Guid PositionId { get; set; }
    public string EmploymentType { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public int YearsOfService { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? PersonalEmail { get; set; }
}

/// <summary>
/// Paginated result wrapper
/// </summary>
public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

/// <summary>
/// Query to get employee details with all related information
/// </summary>
[RequiresModule("HRM")]
public class GetEmployeeDetailsQuery : IRequest<Result<EmployeeDetailsDto>>
{
    public Guid EmployeeId { get; set; }
}

/// <summary>
/// Handler for GetEmployeeDetailsQuery
/// </summary>
public class GetEmployeeDetailsHandler : IRequestHandler<GetEmployeeDetailsQuery, Result<EmployeeDetailsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEmployeeDetailsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<EmployeeDetailsDto>> Handle(
        GetEmployeeDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Include(e => e.Position)
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && !e.IsDeleted, cancellationToken);

        if (employee == null)
            return Result<EmployeeDetailsDto>.Failure("Employee not found");

        // Get leave balances
        var leaveBalances = await _context.Set<LeaveBalance>()
            .AsNoTracking()
            .Where(lb => lb.EmployeeId == request.EmployeeId && lb.Year == DateTime.UtcNow.Year)
            .Select(lb => new LeaveBalanceDto
            {
                LeaveType = lb.LeaveType.ToString(),
                TotalDays = lb.TotalDays,
                UsedDays = lb.UsedDays,
                PendingDays = lb.PendingDays,
                Balance = lb.Balance
            })
            .ToListAsync(cancellationToken);

        // Get recent attendance
        var recentAttendance = await _context.Set<Attendance>()
            .AsNoTracking()
            .Where(a => a.EmployeeId == request.EmployeeId)
            .OrderByDescending(a => a.Date)
            .Take(5)
            .Select(a => new AttendanceListDto
            {
                Id = a.Id,
                Date = a.Date,
                Status = a.Status.ToString(),
                CheckInTime = a.CheckInTime,
                CheckOutTime = a.CheckOutTime,
                WorkingHours = a.WorkingHours
            })
            .ToListAsync(cancellationToken);

        return Result<EmployeeDetailsDto>.Success(new EmployeeDetailsDto
        {
            Id = employee.Id,
            EmployeeNumber = employee.EmployeeNumber,
            FullName = employee.FullName,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            DateOfBirth = employee.DateOfBirth,
            Gender = employee.Gender.ToString(),
            MaritalStatus = employee.MaritalStatus.ToString(),
            Status = employee.Status.ToString(),
            Department = employee.Department?.Name ?? "N/A",
            Position = employee.Position?.Title ?? "N/A",
            EmploymentType = employee.EmploymentType.ToString(),
            HireDate = employee.HireDate,
            ConfirmationDate = employee.ConfirmationDate,
            TerminationDate = employee.TerminationDate,
            YearsOfService = employee.YearsOfService,
            PersonalEmail = employee.PersonalEmail,
            Phone = employee.Phone,
            Mobile = employee.Mobile,
            EmergencyContactName = employee.EmergencyContactName,
            EmergencyContactPhone = employee.EmergencyContactPhone,
            EmergencyContactRelation = employee.EmergencyContactRelation,
            Address = employee.Address,
            City = employee.City,
            Country = employee.Country,
            BankName = employee.BankName,
            BankAccountNumber = employee.BankAccountNumber,
            TaxId = employee.TaxId,
            LeaveBalances = leaveBalances,
            RecentAttendance = recentAttendance
        });
    }
}

/// <summary>
/// Employee details DTO
/// </summary>
public class EmployeeDetailsDto
{
    public Guid Id { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string MaritalStatus { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public DateTime? ConfirmationDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public int YearsOfService { get; set; }
    public string? PersonalEmail { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? TaxId { get; set; }
    public List<LeaveBalanceDto> LeaveBalances { get; set; } = new();
    public List<AttendanceListDto> RecentAttendance { get; set; } = new();
}

/// <summary>
/// Leave balance DTO
/// </summary>
public class LeaveBalanceDto
{
    public string LeaveType { get; set; } = string.Empty;
    public decimal TotalDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal PendingDays { get; set; }
    public decimal Balance { get; set; }
}

/// <summary>
/// Attendance list DTO
/// </summary>
public class AttendanceListDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public TimeSpan? WorkingHours { get; set; }
}

/// <summary>
/// Query to get leave balance summary
/// </summary>
[RequiresModule("HRM")]
public class GetLeaveBalanceSummaryQuery : IRequest<Result<LeaveBalanceSummaryDto>>
{
    public Guid EmployeeId { get; set; }
    public int? Year { get; set; }
}

/// <summary>
/// Handler for GetLeaveBalanceSummaryQuery
/// </summary>
public class GetLeaveBalanceSummaryHandler : IRequestHandler<GetLeaveBalanceSummaryQuery, Result<LeaveBalanceSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLeaveBalanceSummaryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<LeaveBalanceSummaryDto>> Handle(
        GetLeaveBalanceSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var year = request.Year ?? DateTime.UtcNow.Year;

        var balances = await _context.Set<LeaveBalance>()
            .AsNoTracking()
            .Where(lb => lb.EmployeeId == request.EmployeeId && lb.Year == year)
            .Select(lb => new LeaveBalanceDto
            {
                LeaveType = lb.LeaveType.ToString(),
                TotalDays = lb.TotalDays,
                UsedDays = lb.UsedDays,
                PendingDays = lb.PendingDays,
                Balance = lb.Balance
            })
            .ToListAsync(cancellationToken);

        return Result<LeaveBalanceSummaryDto>.Success(new LeaveBalanceSummaryDto
        {
            EmployeeId = request.EmployeeId,
            Year = year,
            Balances = balances,
            TotalAvailable = balances.Sum(b => b.Balance),
            TotalUsed = balances.Sum(b => b.UsedDays)
        });
    }
}

/// <summary>
/// Leave balance summary DTO
/// </summary>
public class LeaveBalanceSummaryDto
{
    public Guid EmployeeId { get; set; }
    public int Year { get; set; }
    public List<LeaveBalanceDto> Balances { get; set; } = new();
    public decimal TotalAvailable { get; set; }
    public decimal TotalUsed { get; set; }
}

/// <summary>
/// Query to get attendance report
/// </summary>
[RequiresModule("HRM")]
public class GetAttendanceReportQuery : IRequest<Result<AttendanceReportDto>>
{
    public Guid? DepartmentId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Handler for GetAttendanceReportQuery
/// </summary>
public class GetAttendanceReportHandler : IRequestHandler<GetAttendanceReportQuery, Result<AttendanceReportDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAttendanceReportHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AttendanceReportDto>> Handle(
        GetAttendanceReportQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Set<Attendance>()
            .AsNoTracking()
            .Where(a => a.Date >= request.StartDate && a.Date <= request.EndDate);

        if (request.DepartmentId.HasValue)
        {
            query = query.Where(a => _context.Employees
                .Any(e => e.Id == a.EmployeeId && e.DepartmentId == request.DepartmentId.Value));
        }

        var attendances = await query
            .Include(a => a.Employee)
            .ToListAsync(cancellationToken);

        var report = new AttendanceReportDto
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalDays = (request.EndDate - request.StartDate).Days + 1,
            TotalRecords = attendances.Count,
            PresentCount = attendances.Count(a => a.Status == AttendanceStatus.Present),
            AbsentCount = attendances.Count(a => a.Status == AttendanceStatus.Absent),
            LateCount = attendances.Count(a => a.Status == AttendanceStatus.Late),
            OnLeaveCount = attendances.Count(a => a.Status == AttendanceStatus.OnLeave),
            HolidayCount = attendances.Count(a => a.Status == AttendanceStatus.Holiday)
        };

        // Calculate average working hours
        var withWorkingHours = attendances.Where(a => a.WorkingHours.HasValue).ToList();
        if (withWorkingHours.Any())
        {
            report.AverageWorkingHours = (decimal)withWorkingHours
                .Average(a => a.WorkingHours!.Value.TotalHours);
        }

        // Group by employee
        report.EmployeeSummaries = attendances
            .GroupBy(a => a.EmployeeId)
            .Select(g => new EmployeeAttendanceSummary
            {
                EmployeeId = g.Key,
                EmployeeName = g.First().Employee?.FullName ?? "Unknown",
                TotalDays = g.Count(),
                PresentDays = g.Count(a => a.Status == AttendanceStatus.Present),
                LateDays = g.Count(a => a.Status == AttendanceStatus.Late),
                AbsentDays = g.Count(a => a.Status == AttendanceStatus.Absent),
                OnLeaveDays = g.Count(a => a.Status == AttendanceStatus.OnLeave),
                AverageWorkingHours = g.Where(a => a.WorkingHours.HasValue)
                    .Select(a => (decimal)a.WorkingHours!.Value.TotalHours)
                    .DefaultIfEmpty(0)
                    .Average()
            })
            .OrderByDescending(e => e.LateDays)
            .ThenByDescending(e => e.AbsentDays)
            .ToList();

        return Result<AttendanceReportDto>.Success(report);
    }
}

/// <summary>
/// Attendance report DTO
/// </summary>
public class AttendanceReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public int TotalRecords { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public int OnLeaveCount { get; set; }
    public int HolidayCount { get; set; }
    public decimal AverageWorkingHours { get; set; }
    public List<EmployeeAttendanceSummary> EmployeeSummaries { get; set; } = new();
}

/// <summary>
/// Employee attendance summary
/// </summary>
public class EmployeeAttendanceSummary
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int TotalDays { get; set; }
    public int PresentDays { get; set; }
    public int LateDays { get; set; }
    public int AbsentDays { get; set; }
    public int OnLeaveDays { get; set; }
    public decimal AverageWorkingHours { get; set; }
}
