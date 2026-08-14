using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Behaviors;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Hrm.Entities;
using ERP.Domain.Hrm.Enums;

namespace ERP.Application.Hrm.Commands.Employees;

/// <summary>
/// Command to update an existing employee
/// </summary>
[RequiresModule("HRM")]
public class UpdateEmployeeCommand : ICommand
{
    public Guid EmployeeId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? MaritalStatus { get; set; }
    public string? PersonalEmail { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? PositionId { get; set; }
    public string? EmploymentType { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankAccountName { get; set; }
    public string? TaxId { get; set; }
}

/// <summary>
/// Validator for UpdateEmployeeCommand
/// </summary>
public class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required");

        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters")
            .When(x => x.FirstName != null);

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters")
            .When(x => x.LastName != null);

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow.AddYears(-18))
            .WithMessage("Employee must be at least 18 years old")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.Gender)
            .Must(g => Enum.TryParse<Gender>(g, true, out _))
            .WithMessage("Invalid gender")
            .When(x => x.Gender != null);

        RuleFor(x => x.MaritalStatus)
            .Must(ms => Enum.TryParse<MaritalStatus>(ms, true, out _))
            .WithMessage("Invalid marital status")
            .When(x => x.MaritalStatus != null);

        RuleFor(x => x.EmploymentType)
            .Must(et => Enum.TryParse<EmploymentType>(et, true, out _))
            .WithMessage("Invalid employment type")
            .When(x => x.EmploymentType != null);
    }
}

/// <summary>
/// Handler for UpdateEmployeeCommand
/// </summary>
public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateEmployeeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Set<Employee>()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && !e.IsDeleted, cancellationToken);

        if (employee == null)
            return Result.Failure("Employee not found");

        // Update personal info
        if (request.FirstName != null || request.LastName != null ||
            request.DateOfBirth.HasValue || request.Gender != null || request.MaritalStatus != null)
        {
            Gender? gender = null;
            MaritalStatus? maritalStatus = null;

            if (request.Gender != null && Enum.TryParse<Gender>(request.Gender, true, out var g))
                gender = g;

            if (request.MaritalStatus != null && Enum.TryParse<MaritalStatus>(request.MaritalStatus, true, out var ms))
                maritalStatus = ms;

            employee.UpdatePersonalInfo(
                request.FirstName,
                request.LastName,
                request.DateOfBirth,
                gender,
                maritalStatus);
        }

        // Update contact info
        if (request.PersonalEmail != null || request.Phone != null || request.Mobile != null)
        {
            employee.UpdateContactInfo(request.Phone, request.Mobile, request.PersonalEmail);
        }

        // Update emergency contact
        if (request.EmergencyContactName != null || request.EmergencyContactPhone != null || request.EmergencyContactRelation != null)
        {
            employee.UpdateEmergencyContact(
                request.EmergencyContactName,
                request.EmergencyContactPhone,
                request.EmergencyContactRelation);
        }

        // Update address
        if (request.Address != null || request.City != null || request.Country != null || request.PostalCode != null)
        {
            employee.UpdateAddress(request.Address, request.City, request.Country, request.PostalCode);
        }

        // Update employment
        if (request.DepartmentId.HasValue || request.PositionId.HasValue || request.EmploymentType != null)
        {
            var departmentId = request.DepartmentId ?? employee.DepartmentId;
            var positionId = request.PositionId ?? employee.PositionId;
            var employmentType = employee.EmploymentType;

            if (request.EmploymentType != null && Enum.TryParse<EmploymentType>(request.EmploymentType, true, out var et))
                employmentType = et;

            employee.UpdateEmployment(departmentId, positionId, employmentType);
        }

        // Update banking info
        if (request.BankName != null || request.BankAccountNumber != null ||
            request.BankAccountName != null || request.TaxId != null)
        {
            employee.UpdateBankingInfo(
                request.BankName,
                request.BankAccountNumber,
                request.BankAccountName,
                request.TaxId);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

/// <summary>
/// Command to update employee status
/// </summary>
[RequiresModule("HRM")]
public class UpdateEmployeeStatusCommand : ICommand
{
    public Guid EmployeeId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime? EffectiveDate { get; set; }
}

/// <summary>
/// Validator for UpdateEmployeeStatusCommand
/// </summary>
public class UpdateEmployeeStatusCommandValidator : AbstractValidator<UpdateEmployeeStatusCommand>
{
    public UpdateEmployeeStatusCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(s => Enum.TryParse<EmployeeStatus>(s, true, out _))
            .WithMessage("Invalid employee status");
    }
}

/// <summary>
/// Handler for UpdateEmployeeStatusCommand
/// </summary>
public class UpdateEmployeeStatusCommandHandler : IRequestHandler<UpdateEmployeeStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateEmployeeStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateEmployeeStatusCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Set<Employee>()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && !e.IsDeleted, cancellationToken);

        if (employee == null)
            return Result.Failure("Employee not found");

        if (!Enum.TryParse<EmployeeStatus>(request.Status, true, out var status))
            return Result.Failure("Invalid employee status");

        var effectiveDate = request.EffectiveDate ?? DateTime.UtcNow;

        switch (status)
        {
            case EmployeeStatus.Terminated:
                employee.Terminate(effectiveDate, request.Reason);
                break;
            case EmployeeStatus.Resigned:
                employee.Resign(effectiveDate);
                break;
            case EmployeeStatus.Active:
                employee.Activate();
                break;
            case EmployeeStatus.Suspended:
                employee.Suspend();
                break;
            case EmployeeStatus.OnLeave:
                employee.SetOnLeave();
                break;
            case EmployeeStatus.Confirmed:
                employee.Confirm();
                break;
            default:
                employee.SetStatus(status);
                break;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

/// <summary>
/// Command to delete (soft delete) an employee
/// </summary>
[RequiresModule("HRM")]
public class DeleteEmployeeCommand : ICommand
{
    public Guid EmployeeId { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Validator for DeleteEmployeeCommand
/// </summary>
public class DeleteEmployeeCommandValidator : AbstractValidator<DeleteEmployeeCommand>
{
    public DeleteEmployeeCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required");
    }
}

/// <summary>
/// Handler for DeleteEmployeeCommand
/// </summary>
public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteEmployeeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Set<Employee>()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && !e.IsDeleted, cancellationToken);

        if (employee == null)
            return Result.Failure("Employee not found");

        // Check if employee has active leave requests or attendance
        var hasActiveLeave = await _context.Set<LeaveRequest>()
            .AnyAsync(lr => lr.EmployeeId == request.EmployeeId &&
                           (lr.Status == LeaveStatus.Pending || lr.Status == LeaveStatus.Approved),
                       cancellationToken);

        if (hasActiveLeave)
            return Result.Failure("Cannot delete employee with active leave requests");

        _context.Set<Employee>().Remove(employee);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
