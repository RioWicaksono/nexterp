using ERP.Application.Common.DTOs;
using ERP.Domain.Hrm.Enums;

namespace ERP.Application.Hrm.DTOs;

/// <summary>
/// Employee DTO
/// </summary>
public class EmployeeDto : BaseDto
{
    public Guid OrganizationId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string FullName => string.IsNullOrEmpty(LastName) ? FirstName : $"{FirstName} {LastName}";
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string MaritalStatus { get; set; } = string.Empty;
    public string? PersonalEmail { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public Guid DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid PositionId { get; set; }
    public string? PositionTitle { get; set; }
    public string EmploymentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public int YearsOfService { get; set; }
}

/// <summary>
/// Department DTO
/// </summary>
public class DepartmentDto : BaseDto
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public string? ParentDepartmentName { get; set; }
    public bool IsActive { get; set; }
    public int EmployeeCount { get; set; }
}

/// <summary>
/// Position DTO
/// </summary>
public class PositionDto : BaseDto
{
    public Guid OrganizationId { get; set; }
    public Guid DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Grade { get; set; }
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Attendance DTO
/// </summary>
public class AttendanceDto : BaseDto
{
    public Guid OrganizationId { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string? WorkingHours { get; set; }
    public decimal? OvertimeHours { get; set; }
    public string? Notes { get; set; }
    public bool IsApproved { get; set; }
}

/// <summary>
/// Leave Request DTO
/// </summary>
public class LeaveRequestDto : BaseDto
{
    public Guid OrganizationId { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public decimal HalfDay { get; set; }
    public decimal TotalLeaveDays { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? RejectionReason { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

/// <summary>
/// Leave Balance DTO
/// </summary>
public class LeaveBalanceDto
{
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal TotalDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal PendingDays { get; set; }
    public decimal Balance { get; set; }
    public decimal CarryForward { get; set; }
}

/// <summary>
/// DTO for creating an employee
/// </summary>
public class CreateEmployeeDto
{
    public Guid UserId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = "Male";
    public string MaritalStatus { get; set; } = "Single";
    public Guid DepartmentId { get; set; }
    public Guid PositionId { get; set; }
    public string EmploymentType { get; set; } = "FullTime";
    public DateTime HireDate { get; set; }
    public string? PersonalEmail { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
}

/// <summary>
/// DTO for creating a leave request
/// </summary>
public class CreateLeaveRequestDto
{
    public Guid EmployeeId { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
    public decimal HalfDay { get; set; }
}

/// <summary>
/// DTO for recording attendance
/// </summary>
public class RecordAttendanceDto
{
    public Guid EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = "Present";
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string? Notes { get; set; }
}
