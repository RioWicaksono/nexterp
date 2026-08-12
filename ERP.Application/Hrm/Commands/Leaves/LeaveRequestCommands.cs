using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Application.Hrm.DTOs;
using ERP.Domain.Hrm.Entities;
using ERP.Domain.Hrm.Enums;

namespace ERP.Application.Hrm.Commands.Leaves;

/// <summary>
/// Command to create a leave request
/// </summary>
public class CreateLeaveRequestCommand : ICommand<Guid>
{
    public Guid EmployeeId { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
    public decimal HalfDay { get; set; }
}

/// <summary>
/// Validator for CreateLeaveRequestCommand
/// </summary>
public class CreateLeaveRequestCommandValidator : AbstractValidator<CreateLeaveRequestCommand>
{
    public CreateLeaveRequestCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee is required");

        RuleFor(x => x.LeaveType)
            .Must(lt => Enum.TryParse<LeaveType>(lt, true, out _))
            .WithMessage("Invalid leave type");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Start date cannot be in the past");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date cannot be before start date");

        RuleFor(x => x.HalfDay)
            .InclusiveBetween(0, 1)
            .WithMessage("Half day must be between 0 and 1");
    }
}

/// <summary>
/// Handler for CreateLeaveRequestCommand
/// </summary>
public class CreateLeaveRequestCommandHandler : IRequestHandler<CreateLeaveRequestCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateLeaveRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        // Get employee organization
        var employee = await _context.Set<Employee>()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && !e.IsDeleted, cancellationToken);

        if (employee == null)
            return Result<Guid>.Failure("Employee not found");

        if (employee.Status == Domain.Hrm.Enums.EmployeeStatus.Terminated ||
            employee.Status == Domain.Hrm.Enums.EmployeeStatus.Resigned)
            return Result<Guid>.Failure("Cannot create leave request for inactive employee");

        // Parse leave type
        if (!Enum.TryParse<LeaveType>(request.LeaveType, true, out var leaveType))
            return Result<Guid>.Failure("Invalid leave type");

        // Check leave balance
        var currentYear = DateTime.UtcNow.Year;
        var leaveBalance = await _context.Set<LeaveBalance>()
            .FirstOrDefaultAsync(lb =>
                lb.EmployeeId == request.EmployeeId &&
                lb.LeaveType == leaveType &&
                lb.Year == currentYear, cancellationToken);

        if (leaveBalance != null)
        {
            var requestedDays = (decimal)((request.EndDate - request.StartDate).Days + 1) - request.HalfDay;
            if (requestedDays > leaveBalance.Balance)
                return Result<Guid>.Failure($"Insufficient leave balance. Available: {leaveBalance.Balance}, Requested: {requestedDays}");
        }

        // Check for overlapping leave requests
        var overlappingLeave = await _context.Set<LeaveRequest>()
            .AnyAsync(lr =>
                lr.EmployeeId == request.EmployeeId &&
                lr.StartDate <= request.EndDate &&
                lr.EndDate >= request.StartDate &&
                lr.Status != LeaveStatus.Cancelled &&
                lr.Status != LeaveStatus.Rejected, cancellationToken);

        if (overlappingLeave)
            return Result<Guid>.Failure("Leave request overlaps with existing request");

        // Create leave request
        var leaveRequest = LeaveRequest.Create(
            employee.OrganizationId,
            request.EmployeeId,
            leaveType,
            request.StartDate,
            request.EndDate,
            request.Reason,
            request.HalfDay);

        _context.Set<LeaveRequest>().Add(leaveRequest);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(leaveRequest.Id);
    }
}

/// <summary>
/// Command to approve/reject leave request
/// </summary>
public class ApproveLeaveRequestCommand : ICommand
{
    public Guid LeaveRequestId { get; set; }
    public Guid ApproverId { get; set; }
    public bool Approved { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Validator for ApproveLeaveRequestCommand
/// </summary>
public class ApproveLeaveRequestCommandValidator : AbstractValidator<ApproveLeaveRequestCommand>
{
    public ApproveLeaveRequestCommandValidator()
    {
        RuleFor(x => x.LeaveRequestId)
            .NotEmpty().WithMessage("Leave request ID is required");

        RuleFor(x => x.ApproverId)
            .NotEmpty().WithMessage("Approver ID is required");

        RuleFor(x => x.Reason)
            .NotEmpty().When(x => !x.Approved)
            .WithMessage("Rejection reason is required");
    }
}

/// <summary>
/// Handler for ApproveLeaveRequestCommand
/// </summary>
public class ApproveLeaveRequestCommandHandler : IRequestHandler<ApproveLeaveRequestCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ApproveLeaveRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ApproveLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = await _context.Set<LeaveRequest>()
            .FirstOrDefaultAsync(lr => lr.Id == request.LeaveRequestId, cancellationToken);

        if (leaveRequest == null)
            return Result.Failure("Leave request not found");

        if (!leaveRequest.IsPending)
            return Result.Failure("Leave request is not pending");

        if (request.Approved)
        {
            leaveRequest.Approve(request.ApproverId);

            // Update leave balance
            var currentYear = DateTime.UtcNow.Year;
            var leaveBalance = await _context.Set<LeaveBalance>()
                .FirstOrDefaultAsync(lb =>
                    lb.EmployeeId == leaveRequest.EmployeeId &&
                    lb.LeaveType == leaveRequest.LeaveType &&
                    lb.Year == currentYear, cancellationToken);

            if (leaveBalance != null)
            {
                leaveBalance.UseDays(leaveRequest.TotalLeaveDays);
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
                return Result.Failure("Rejection reason is required");

            leaveRequest.Reject(request.ApproverId, request.Reason);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
