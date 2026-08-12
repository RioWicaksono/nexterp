using ERP.Domain.Common;
using ERP.Domain.Hrm.Enums;

namespace ERP.Domain.Hrm.Entities;

/// <summary>
/// Attendance record entity
/// </summary>
public class Attendance : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public DateTime Date { get; private set; }
    public AttendanceStatus Status { get; private set; }
    public DateTime? CheckInTime { get; private set; }
    public DateTime? CheckOutTime { get; private set; }
    public TimeSpan? WorkingHours => CheckOutTime.HasValue && CheckInTime.HasValue
        ? CheckOutTime.Value - CheckInTime.Value
        : null;
    public TimeSpan? ExpectedHours { get; private set; }
    public decimal? OvertimeHours { get; private set; }
    public string? Notes { get; private set; }
    public string? Location { get; private set; }
    public bool IsApproved { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }

    // Navigation properties
    private readonly Employee? _employee;
    public Employee? Employee => _employee;

    // Factory method
    public static Attendance Create(
        Guid organizationId,
        Guid employeeId,
        DateTime date,
        AttendanceStatus status,
        DateTime? checkInTime = null,
        DateTime? checkOutTime = null,
        string? notes = null)
    {
        return new Attendance
        {
            OrganizationId = organizationId,
            EmployeeId = employeeId,
            Date = date.Date,
            Status = status,
            CheckInTime = checkInTime,
            CheckOutTime = checkOutTime,
            Notes = notes?.Trim()
        };
    }

    public void CheckIn(DateTime checkInTime, string? location = null)
    {
        if (CheckInTime.HasValue)
            throw new InvalidOperationException("Already checked in");

        CheckInTime = checkInTime;
        Location = location?.Trim();
        Status = AttendanceStatus.Present;

        // Check if late (after 9 AM by default)
        if (checkInTime.TimeOfDay > new TimeSpan(9, 0, 0))
            Status = AttendanceStatus.Late;

        UpdateTimestamp();
    }

    public void CheckOut(DateTime checkOutTime)
    {
        if (!CheckInTime.HasValue)
            throw new InvalidOperationException("Not checked in yet");

        if (checkOutTime < CheckInTime.Value)
            throw new InvalidOperationException("Check out time cannot be before check in time");

        CheckOutTime = checkOutTime;
        Status = AttendanceStatus.Present;
        UpdateTimestamp();
    }

    public void SetOvertime(decimal hours)
    {
        if (hours < 0)
            throw new ArgumentException("Overtime hours cannot be negative", nameof(hours));

        OvertimeHours = hours;
        UpdateTimestamp();
    }

    public void SetExpectedHours(TimeSpan hours)
    {
        ExpectedHours = hours;
        UpdateTimestamp();
    }

    public void Approve(Guid approvedBy)
    {
        if (IsApproved)
            throw new InvalidOperationException("Already approved");

        IsApproved = true;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void Reject()
    {
        IsApproved = false;
        UpdateTimestamp();
    }

    public void SetStatus(AttendanceStatus status)
    {
        Status = status;
        UpdateTimestamp();
    }

    public void SetNotes(string? notes)
    {
        Notes = notes?.Trim();
        UpdateTimestamp();
    }
}

/// <summary>
/// Leave Request entity
/// </summary>
public class LeaveRequest : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public LeaveType LeaveType { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public int TotalDays => (EndDate - StartDate).Days + 1;
    public decimal HalfDay { get; private set; }  // 0 = full day, 0.5 = half day
    public LeaveStatus Status { get; private set; } = LeaveStatus.Pending;
    public string? Reason { get; private set; }
    public string? RejectionReason { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public Guid? RejectedBy { get; private set; }
    public DateTime? RejectedAt { get; private set; }

    // Navigation properties
    private readonly Employee? _employee;
    public Employee? Employee => _employee;

    // Calculated properties
    public decimal TotalLeaveDays => TotalDays - (decimal)HalfDay;
    public bool IsPending => Status == LeaveStatus.Pending;
    public bool IsApproved => Status == LeaveStatus.Approved;
    public bool IsRejected => Status == LeaveStatus.Rejected;
    public bool IsActive => Status != LeaveStatus.Cancelled && Status != LeaveStatus.Rejected;

    // Factory method
    public static LeaveRequest Create(
        Guid organizationId,
        Guid employeeId,
        LeaveType leaveType,
        DateTime startDate,
        DateTime endDate,
        string? reason = null,
        decimal halfDay = 0)
    {
        if (endDate < startDate)
            throw new ArgumentException("End date cannot be before start date", nameof(endDate));

        if (halfDay < 0 || halfDay > 1)
            throw new ArgumentException("Half day must be between 0 and 1", nameof(halfDay));

        return new LeaveRequest
        {
            OrganizationId = organizationId,
            EmployeeId = employeeId,
            LeaveType = leaveType,
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            Reason = reason?.Trim(),
            HalfDay = halfDay
        };
    }

    public void Approve(Guid approvedBy)
    {
        if (!IsPending)
            throw new InvalidOperationException("Can only approve pending requests");

        Status = LeaveStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void Reject(Guid rejectedBy, string reason)
    {
        if (!IsPending)
            throw new InvalidOperationException("Can only reject pending requests");

        Status = LeaveStatus.Rejected;
        RejectedBy = rejectedBy;
        RejectedAt = DateTime.UtcNow;
        RejectionReason = reason?.Trim();
        UpdateTimestamp();
    }

    public void Cancel()
    {
        if (IsApproved)
            throw new InvalidOperationException("Cannot cancel approved leave");

        Status = LeaveStatus.Cancelled;
        UpdateTimestamp();
    }

    public void UpdateDates(DateTime startDate, DateTime endDate)
    {
        if (!IsPending)
            throw new InvalidOperationException("Can only update pending requests");

        if (endDate < startDate)
            throw new ArgumentException("End date cannot be before start date");

        StartDate = startDate.Date;
        EndDate = endDate.Date;
        UpdateTimestamp();
    }

    public void SetReason(string? reason)
    {
        Reason = reason?.Trim();
        UpdateTimestamp();
    }
}

/// <summary>
/// Leave Balance entity
/// </summary>
public class LeaveBalance : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public LeaveType LeaveType { get; private set; }
    public int Year { get; private set; }
    public decimal TotalDays { get; private set; }
    public decimal UsedDays { get; private set; }
    public decimal PendingDays { get; private set; }
    public decimal Balance => TotalDays - UsedDays - PendingDays;
    public decimal CarryForward { get; private set; }

    // Factory method
    public static LeaveBalance Create(
        Guid organizationId,
        Guid employeeId,
        LeaveType leaveType,
        int year,
        decimal totalDays,
        decimal carryForward = 0)
    {
        return new LeaveBalance
        {
            OrganizationId = organizationId,
            EmployeeId = employeeId,
            LeaveType = leaveType,
            Year = year,
            TotalDays = totalDays,
            CarryForward = carryForward
        };
    }

    public void AddAllocation(decimal days)
    {
        if (days <= 0)
            throw new ArgumentException("Days must be positive", nameof(days));

        TotalDays += days;
        UpdateTimestamp();
    }

    public void UseDays(decimal days)
    {
        if (days <= 0)
            throw new ArgumentException("Days must be positive", nameof(days));

        if (days > Balance)
            throw new InvalidOperationException($"Insufficient leave balance. Available: {Balance}, Requested: {days}");

        UsedDays += days;
        UpdateTimestamp();
    }

    public void SetPendingDays(decimal days)
    {
        PendingDays = days;
        UpdateTimestamp();
    }

    public void AdjustCarryForward(decimal days)
    {
        CarryForward = days;
        UpdateTimestamp();
    }
}
