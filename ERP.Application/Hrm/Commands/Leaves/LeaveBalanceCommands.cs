using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Behaviors;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Hrm.Entities;
using ERP.Domain.Hrm.Enums;

namespace ERP.Application.Hrm.Commands.Leaves;

/// <summary>
/// Command to create or update leave balance for an employee
/// </summary>
[RequiresModule("HRM")]
public class SetLeaveBalanceCommand : ICommand
{
    public Guid EmployeeId { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal TotalDays { get; set; }
    public decimal CarryForwardDays { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Validator for SetLeaveBalanceCommand
/// </summary>
public class SetLeaveBalanceCommandValidator : AbstractValidator<SetLeaveBalanceCommand>
{
    public SetLeaveBalanceCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee is required");

        RuleFor(x => x.LeaveType)
            .NotEmpty().WithMessage("Leave type is required")
            .Must(lt => Enum.TryParse<LeaveType>(lt, true, out _))
            .WithMessage("Invalid leave type");

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100)
            .WithMessage("Year must be between 2000 and 2100");

        RuleFor(x => x.TotalDays)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total days cannot be negative");

        RuleFor(x => x.CarryForwardDays)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Carry forward days cannot be negative");
    }
}

/// <summary>
/// Handler for SetLeaveBalanceCommand
/// </summary>
public class SetLeaveBalanceCommandHandler : IRequestHandler<SetLeaveBalanceCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public SetLeaveBalanceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(SetLeaveBalanceCommand request, CancellationToken cancellationToken)
    {
        // Get employee organization
        var employee = await _context.Set<Employee>()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && !e.IsDeleted, cancellationToken);

        if (employee == null)
            return Result.Failure("Employee not found");

        // Parse leave type
        if (!Enum.TryParse<LeaveType>(request.LeaveType, true, out var leaveType))
            return Result.Failure("Invalid leave type");

        // Find or create leave balance
        var leaveBalance = await _context.Set<LeaveBalance>()
            .FirstOrDefaultAsync(lb =>
                lb.EmployeeId == request.EmployeeId &&
                lb.LeaveType == leaveType &&
                lb.Year == request.Year, cancellationToken);

        if (leaveBalance != null)
        {
            // Update existing balance
            leaveBalance.AddAllocation(request.TotalDays - leaveBalance.TotalDays);
            leaveBalance.AdjustCarryForward(request.CarryForwardDays);
        }
        else
        {
            // Create new balance
            leaveBalance = LeaveBalance.Create(
                employee.OrganizationId,
                request.EmployeeId,
                leaveType,
                request.Year,
                request.TotalDays,
                request.CarryForwardDays);

            _context.Set<LeaveBalance>().Add(leaveBalance);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

/// <summary>
/// Command to auto-allocate leave balance based on years of service
/// </summary>
[RequiresModule("HRM")]
public class AutoAllocateLeaveBalanceCommand : ICommand
{
    public Guid EmployeeId { get; set; }
    public int Year { get; set; }
}

/// <summary>
/// Validator for AutoAllocateLeaveBalanceCommand
/// </summary>
public class AutoAllocateLeaveBalanceCommandValidator : AbstractValidator<AutoAllocateLeaveBalanceCommand>
{
    public AutoAllocateLeaveBalanceCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee is required");

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100)
            .WithMessage("Year must be between 2000 and 2100");
    }
}

/// <summary>
/// Handler for AutoAllocateLeaveBalanceCommand
/// </summary>
public class AutoAllocateLeaveBalanceCommandHandler : IRequestHandler<AutoAllocateLeaveBalanceCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public AutoAllocateLeaveBalanceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(AutoAllocateLeaveBalanceCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Set<Employee>()
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && !e.IsDeleted, cancellationToken);

        if (employee == null)
            return Result.Failure("Employee not found");

        // Calculate annual leave based on years of service
        var annualDays = LeaveEntitlement.CalculateAnnualLeaveDays(employee.YearsOfService);

        // Find or create annual leave balance
        var leaveBalance = await _context.Set<LeaveBalance>()
            .FirstOrDefaultAsync(lb =>
                lb.EmployeeId == request.EmployeeId &&
                lb.LeaveType == LeaveType.Annual &&
                lb.Year == request.Year, cancellationToken);

        if (leaveBalance != null)
        {
            // Add allocation if not already set
            if (leaveBalance.TotalDays < annualDays)
            {
                leaveBalance.AddAllocation(annualDays - leaveBalance.TotalDays);
            }
        }
        else
        {
            leaveBalance = LeaveBalance.Create(
                employee.OrganizationId,
                request.EmployeeId,
                LeaveType.Annual,
                request.Year,
                annualDays);

            _context.Set<LeaveBalance>().Add(leaveBalance);
        }

        // Allocate default sick leave (max 14 days per year)
        var sickBalance = await _context.Set<LeaveBalance>()
            .FirstOrDefaultAsync(lb =>
                lb.EmployeeId == request.EmployeeId &&
                lb.LeaveType == LeaveType.Sick &&
                lb.Year == request.Year, cancellationToken);

        if (sickBalance == null)
        {
            sickBalance = LeaveBalance.Create(
                employee.OrganizationId,
                request.EmployeeId,
                LeaveType.Sick,
                request.Year,
                14);

            _context.Set<LeaveBalance>().Add(sickBalance);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

/// <summary>
/// Command to cancel a leave request
/// </summary>
[RequiresModule("HRM")]
public class CancelLeaveRequestCommand : ICommand
{
    public Guid LeaveRequestId { get; set; }
}

/// <summary>
/// Validator for CancelLeaveRequestCommand
/// </summary>
public class CancelLeaveRequestCommandValidator : AbstractValidator<CancelLeaveRequestCommand>
{
    public CancelLeaveRequestCommandValidator()
    {
        RuleFor(x => x.LeaveRequestId)
            .NotEmpty().WithMessage("Leave request ID is required");
    }
}

/// <summary>
/// Handler for CancelLeaveRequestCommand
/// </summary>
public class CancelLeaveRequestCommandHandler : IRequestHandler<CancelLeaveRequestCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public CancelLeaveRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(CancelLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = await _context.Set<LeaveRequest>()
            .FirstOrDefaultAsync(lr => lr.Id == request.LeaveRequestId, cancellationToken);

        if (leaveRequest == null)
            return Result.Failure("Leave request not found");

        if (!leaveRequest.IsPending && !leaveRequest.IsApproved)
            return Result.Failure("Leave request cannot be cancelled");

        leaveRequest.Cancel();
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
