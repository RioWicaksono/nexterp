using ERP.Domain.Common;
using ERP.Domain.Common.Modules;
using ERP.Domain.Hrm.Enums;

namespace ERP.Domain.Hrm.Entities;

/// <summary>
/// Overtime request for Indonesian labor law compliance.
/// Max 4 hours/day, 18 hours/week per UU Ketenagakerjaan.
/// </summary>
public class OvertimeRequest : BaseEntity, ITenantEntity
{
	public Guid OrganizationId { get; private set; }
	public Guid EmployeeId { get; private set; }
	public DateTime RequestDate { get; private set; }
	public DateTime WorkDate { get; private set; }
	public TimeSpan StartTime { get; private set; }
	public TimeSpan EndTime { get; private set; }
	public OvertimeType OvertimeType { get; private set; }
	public decimal Hours { get; private set; }
	public decimal ApprovedHours { get; private set; }
	public OvertimeStatus Status { get; private set; } = OvertimeStatus.Pending;
	public string? Reason { get; private set; }
	public string? ApprovalNotes { get; private set; }
	public Guid? ApprovedById { get; private set; }
	public DateTime? ApprovedAt { get; private set; }

	// Navigation
	private readonly Employee _employee = null!;
	public Employee Employee => _employee;

	// Constants for Indonesian labor law
	public const decimal MaxDailyOvertimeHours = 4m;
	public const decimal MaxWeeklyOvertimeHours = 18m;

	private OvertimeRequest() { }

	public static OvertimeRequest Create(
		Guid organizationId,
		Guid employeeId,
		DateTime workDate,
		TimeSpan startTime,
		TimeSpan endTime,
		OvertimeType overtimeType,
		string? reason = null)
	{
		var hours = CalculateHours(startTime, endTime);

		// Validate labor law limits
		if (hours > MaxDailyOvertimeHours)
			hours = MaxDailyOvertimeHours;

		return new OvertimeRequest
		{
			OrganizationId = organizationId,
			EmployeeId = employeeId,
			RequestDate = DateTime.UtcNow,
			WorkDate = workDate,
			StartTime = startTime,
			EndTime = endTime,
			OvertimeType = overtimeType,
			Hours = hours,
			ApprovedHours = 0,
			Status = OvertimeStatus.Pending,
			Reason = reason
		};
	}

	public void Approve(decimal approvedHours, Guid approverId, string? notes = null)
	{
		if (Status != OvertimeStatus.Pending)
			throw new InvalidOperationException("Only pending overtime requests can be approved");

		if (approvedHours > MaxDailyOvertimeHours)
			approvedHours = MaxDailyOvertimeHours;

		ApprovedHours = approvedHours;
		Status = OvertimeStatus.Approved;
		ApprovedById = approverId;
		ApprovedAt = DateTime.UtcNow;
		ApprovalNotes = notes;
		UpdateTimestamp();
	}

	public void Reject(Guid rejectedById, string reason)
	{
		if (Status != OvertimeStatus.Pending)
			throw new InvalidOperationException("Only pending overtime requests can be rejected");

		Status = OvertimeStatus.Rejected;
		ApprovedById = rejectedById;
		ApprovedAt = DateTime.UtcNow;
		ApprovalNotes = reason;
		UpdateTimestamp();
	}

	public void Cancel()
	{
		if (Status == OvertimeStatus.Pending)
			Status = OvertimeStatus.Cancelled;
	}

	private static decimal CalculateHours(TimeSpan start, TimeSpan end)
	{
		var duration = end - start;
		return (decimal)duration.TotalHours;
	}

	public decimal CalculateOvertimePay(decimal hourlyRate, decimal multiplier = 1.5m)
	{
		if (Status != OvertimeStatus.Approved)
			return 0;

		return ApprovedHours * hourlyRate * multiplier;
	}
}
