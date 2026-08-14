using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Behaviors;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Hrm.Entities;
using ERP.Domain.Hrm.Enums;

namespace ERP.Application.Hrm.Commands.Overtimes;

/// <summary>
/// Command to create an overtime request
/// </summary>
[RequiresModule("HRM")]
public class CreateOvertimeRequestCommand : ICommand<Guid>
{
    public Guid EmployeeId { get; set; }
    public DateTime WorkDate { get; set; }
    public string StartTime { get; set; } = string.Empty; // Format: "HH:mm"
    public string EndTime { get; set; } = string.Empty;   // Format: "HH:mm"
    public string OvertimeType { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

/// <summary>
/// Validator for CreateOvertimeRequestCommand
/// </summary>
public class CreateOvertimeRequestCommandValidator : AbstractValidator<CreateOvertimeRequestCommand>
{
    public CreateOvertimeRequestCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee is required");

        RuleFor(x => x.WorkDate)
            .NotEmpty().WithMessage("Work date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1))
            .WithMessage("Work date cannot be more than 1 day in the future");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required")
            .Matches(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$")
            .WithMessage("Invalid start time format. Use HH:mm");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("End time is required")
            .Matches(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$")
            .WithMessage("Invalid end time format. Use HH:mm");

        RuleFor(x => x.OvertimeType)
            .Must(ot => Enum.TryParse<OvertimeType>(ot, true, out _))
            .WithMessage("Invalid overtime type");
    }
}

/// <summary>
/// Handler for CreateOvertimeRequestCommand
/// </summary>
public class CreateOvertimeRequestCommandHandler : IRequestHandler<CreateOvertimeRequestCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateOvertimeRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateOvertimeRequestCommand request, CancellationToken cancellationToken)
    {
        // Get employee organization
        var employee = await _context.Set<Employee>()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && !e.IsDeleted, cancellationToken);

        if (employee == null)
            return Result<Guid>.Failure("Employee not found");

        if (employee.Status == EmployeeStatus.Terminated || employee.Status == EmployeeStatus.Resigned)
            return Result<Guid>.Failure("Cannot create overtime request for inactive employee");

        // Parse time
        if (!TimeSpan.TryParse(request.StartTime, out var startTime))
            return Result<Guid>.Failure("Invalid start time format");

        if (!TimeSpan.TryParse(request.EndTime, out var endTime))
            return Result<Guid>.Failure("Invalid end time format");

        // Parse overtime type
        if (!Enum.TryParse<OvertimeType>(request.OvertimeType, true, out var overtimeType))
            return Result<Guid>.Failure("Invalid overtime type");

        // Calculate hours
        var hours = (decimal)(endTime - startTime).TotalHours;
        if (hours <= 0)
            return Result<Guid>.Failure("End time must be after start time");

        // Check Indonesian labor law limits
        if (hours > OvertimeRequest.MaxDailyOvertimeHours)
            return Result<Guid>.Failure($"Overtime hours cannot exceed {OvertimeRequest.MaxDailyOvertimeHours} hours per day (UU Ketenagakerjaan)");

        // Check for existing overtime request on same date
        var existingOvertime = await _context.Set<OvertimeRequest>()
            .AnyAsync(or =>
                or.EmployeeId == request.EmployeeId &&
                or.WorkDate == request.WorkDate.Date &&
                (or.Status == OvertimeStatus.Pending || or.Status == OvertimeStatus.Approved),
                cancellationToken);

        if (existingOvertime)
            return Result<Guid>.Failure("An overtime request already exists for this date");

        // Create overtime request
        var overtimeRequest = OvertimeRequest.Create(
            employee.OrganizationId,
            request.EmployeeId,
            request.WorkDate,
            startTime,
            endTime,
            overtimeType,
            request.Reason);

        _context.Set<OvertimeRequest>().Add(overtimeRequest);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(overtimeRequest.Id);
    }
}

/// <summary>
/// Command to approve or reject overtime request
/// </summary>
[RequiresModule("HRM")]
public class ApproveOvertimeRequestCommand : ICommand
{
    public Guid OvertimeRequestId { get; set; }
    public Guid ApproverId { get; set; }
    public bool Approved { get; set; }
    public decimal? ApprovedHours { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Validator for ApproveOvertimeRequestCommand
/// </summary>
public class ApproveOvertimeRequestCommandValidator : AbstractValidator<ApproveOvertimeRequestCommand>
{
    public ApproveOvertimeRequestCommandValidator()
    {
        RuleFor(x => x.OvertimeRequestId)
            .NotEmpty().WithMessage("Overtime request ID is required");

        RuleFor(x => x.ApproverId)
            .NotEmpty().WithMessage("Approver ID is required");

        RuleFor(x => x.ApprovedHours)
            .GreaterThan(0)
            .LessThanOrEqualTo(4)
            .WithMessage("Approved hours must be between 0 and 4")
            .When(x => x.Approved);
    }
}

/// <summary>
/// Handler for ApproveOvertimeRequestCommand
/// </summary>
public class ApproveOvertimeRequestCommandHandler : IRequestHandler<ApproveOvertimeRequestCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ApproveOvertimeRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ApproveOvertimeRequestCommand request, CancellationToken cancellationToken)
    {
        var overtimeRequest = await _context.Set<OvertimeRequest>()
            .FirstOrDefaultAsync(or => or.Id == request.OvertimeRequestId, cancellationToken);

        if (overtimeRequest == null)
            return Result.Failure("Overtime request not found");

        if (request.Approved)
        {
            var hours = request.ApprovedHours ?? overtimeRequest.Hours;
            overtimeRequest.Approve(hours, request.ApproverId, request.Notes);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.Notes))
                return Result.Failure("Rejection reason is required");

            overtimeRequest.Reject(request.ApproverId, request.Notes);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

/// <summary>
/// Command to cancel an overtime request
/// </summary>
[RequiresModule("HRM")]
public class CancelOvertimeRequestCommand : ICommand
{
    public Guid OvertimeRequestId { get; set; }
}

/// <summary>
/// Validator for CancelOvertimeRequestCommand
/// </summary>
public class CancelOvertimeRequestCommandValidator : AbstractValidator<CancelOvertimeRequestCommand>
{
    public CancelOvertimeRequestCommandValidator()
    {
        RuleFor(x => x.OvertimeRequestId)
            .NotEmpty().WithMessage("Overtime request ID is required");
    }
}

/// <summary>
/// Handler for CancelOvertimeRequestCommand
/// </summary>
public class CancelOvertimeRequestCommandHandler : IRequestHandler<CancelOvertimeRequestCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public CancelOvertimeRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(CancelOvertimeRequestCommand request, CancellationToken cancellationToken)
    {
        var overtimeRequest = await _context.Set<OvertimeRequest>()
            .FirstOrDefaultAsync(or => or.Id == request.OvertimeRequestId, cancellationToken);

        if (overtimeRequest == null)
            return Result.Failure("Overtime request not found");

        overtimeRequest.Cancel();
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
