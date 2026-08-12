using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Application.Hrm.DTOs;
using ERP.Domain.Hrm.Entities;
using ERP.Domain.Hrm.Enums;

namespace ERP.Application.Hrm.Commands.Attendances;

/// <summary>
/// Command to record attendance
/// </summary>
public class RecordAttendanceCommand : ICommand<Guid>
{
    public Guid EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = "Present";
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Validator for RecordAttendanceCommand
/// </summary>
public class RecordAttendanceCommandValidator : AbstractValidator<RecordAttendanceCommand>
{
    public RecordAttendanceCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee is required");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Date cannot be in the future");

        RuleFor(x => x.Status)
            .Must(s => Enum.TryParse<AttendanceStatus>(s, true, out _))
            .WithMessage("Invalid attendance status. Valid values: Present, Absent, Late, OnLeave, Holiday");

        RuleFor(x => x.CheckInTime)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.CheckInTime.HasValue)
            .WithMessage("Check-in time cannot be in the future");

        RuleFor(x => x.CheckOutTime)
            .GreaterThan(x => x.CheckInTime)
            .When(x => x.CheckInTime.HasValue && x.CheckOutTime.HasValue)
            .WithMessage("Check-out time must be after check-in time");
    }
}

/// <summary>
/// Handler for RecordAttendanceCommand
/// </summary>
public class RecordAttendanceCommandHandler : IRequestHandler<RecordAttendanceCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public RecordAttendanceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(RecordAttendanceCommand request, CancellationToken cancellationToken)
    {
        // Get employee organization
        var employee = await _context.Set<Employee>()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && !e.IsDeleted, cancellationToken);

        if (employee == null)
            return Result<Guid>.Failure("Employee not found");

        // Check for existing attendance on same date
        var existingAttendance = await _context.Set<Attendance>()
            .FirstOrDefaultAsync(a =>
                a.EmployeeId == request.EmployeeId &&
                a.Date == request.Date.Date, cancellationToken);

        if (existingAttendance != null)
            return Result<Guid>.Failure("Attendance already recorded for this date");

        // Parse status
        if (!Enum.TryParse<AttendanceStatus>(request.Status, true, out var status))
            return Result<Guid>.Failure("Invalid attendance status");

        // Create attendance
        var attendance = Attendance.Create(
            employee.OrganizationId,
            request.EmployeeId,
            request.Date,
            status,
            request.CheckInTime,
            request.CheckOutTime,
            request.Notes);

        _context.Set<Attendance>().Add(attendance);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(attendance.Id);
    }
}

/// <summary>
/// Command to check in
/// </summary>
public class CheckInCommand : ICommand
{
    public Guid EmployeeId { get; set; }
    public DateTime CheckInTime { get; set; }
    public string? Location { get; set; }
}

/// <summary>
/// Handler for CheckInCommand
/// </summary>
public class CheckInCommandHandler : IRequestHandler<CheckInCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public CheckInCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(CheckInCommand request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;

        // Find or create today's attendance
        var attendance = await _context.Set<Attendance>()
            .FirstOrDefaultAsync(a =>
                a.EmployeeId == request.EmployeeId &&
                a.Date == today, cancellationToken);

        if (attendance == null)
        {
            var employee = await _context.Set<Employee>()
                .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && !e.IsDeleted, cancellationToken);

            if (employee == null)
                return Result.Failure("Employee not found");

            attendance = Attendance.Create(
                employee.OrganizationId,
                request.EmployeeId,
                today,
                AttendanceStatus.Present,
                request.CheckInTime);
            _context.Set<Attendance>().Add(attendance);
        }

        attendance.CheckIn(request.CheckInTime, request.Location);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

/// <summary>
/// Command to check out
/// </summary>
public class CheckOutCommand : ICommand
{
    public Guid EmployeeId { get; set; }
    public DateTime CheckOutTime { get; set; }
}

/// <summary>
/// Handler for CheckOutCommand
/// </summary>
public class CheckOutCommandHandler : IRequestHandler<CheckOutCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public CheckOutCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(CheckOutCommand request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;

        var attendance = await _context.Set<Attendance>()
            .FirstOrDefaultAsync(a =>
                a.EmployeeId == request.EmployeeId &&
                a.Date == today, cancellationToken);

        if (attendance == null)
            return Result.Failure("No attendance record found for today");

        attendance.CheckOut(request.CheckOutTime);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
