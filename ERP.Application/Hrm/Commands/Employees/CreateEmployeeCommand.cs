using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Application.Hrm.DTOs;
using ERP.Domain.Hrm.Entities;
using ERP.Domain.Hrm.Enums;

namespace ERP.Application.Hrm.Commands.Employees;

/// <summary>
/// Command to create a new employee
/// </summary>
public class CreateEmployeeCommand : ICommand<Guid>
{
    public Guid UserId { get; set; }
    public Guid OrganizationId { get; set; }
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
/// Validator for CreateEmployeeCommand
/// </summary>
public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.EmployeeNumber)
            .NotEmpty().WithMessage("Employee number is required")
            .MaximumLength(50).WithMessage("Employee number cannot exceed 50 characters");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .LessThan(DateTime.UtcNow.AddYears(-18))
            .WithMessage("Employee must be at least 18 years old");

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Department is required");

        RuleFor(x => x.PositionId)
            .NotEmpty().WithMessage("Position is required");

        RuleFor(x => x.HireDate)
            .NotEmpty().WithMessage("Hire date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("Hire date cannot be in the future");

        RuleFor(x => x.Gender)
            .Must(g => Enum.TryParse<Gender>(g, true, out _))
            .WithMessage("Invalid gender. Valid values: Male, Female, Other");

        RuleFor(x => x.MaritalStatus)
            .Must(ms => Enum.TryParse<MaritalStatus>(ms, true, out _))
            .WithMessage("Invalid marital status. Valid values: Single, Married, Divorced, Widowed");

        RuleFor(x => x.EmploymentType)
            .Must(et => Enum.TryParse<EmploymentType>(et, true, out _))
            .WithMessage("Invalid employment type. Valid values: FullTime, PartTime, Contract, Probation, Intern, Freelance");
    }
}

/// <summary>
/// Handler for CreateEmployeeCommand
/// </summary>
public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateEmployeeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        // Check if employee number already exists
        var existingNumber = await _context.Set<Employee>()
            .AnyAsync(e => e.OrganizationId == request.OrganizationId &&
                          e.EmployeeNumber == request.EmployeeNumber.ToUpperInvariant() &&
                          !e.IsDeleted, cancellationToken);

        if (existingNumber)
            return Result<Guid>.Failure("Employee number already exists");

        // Check if department exists
        var departmentExists = await _context.Set<Department>()
            .AnyAsync(d => d.Id == request.DepartmentId && !d.IsDeleted, cancellationToken);

        if (!departmentExists)
            return Result<Guid>.Failure("Department not found");

        // Check if position exists
        var positionExists = await _context.Set<Position>()
            .AnyAsync(p => p.Id == request.PositionId && !p.IsDeleted, cancellationToken);

        if (!positionExists)
            return Result<Guid>.Failure("Position not found");

        // Parse enums
        if (!Enum.TryParse<Gender>(request.Gender, true, out var gender))
            return Result<Guid>.Failure("Invalid gender");

        if (!Enum.TryParse<MaritalStatus>(request.MaritalStatus, true, out var maritalStatus))
            return Result<Guid>.Failure("Invalid marital status");

        if (!Enum.TryParse<EmploymentType>(request.EmploymentType, true, out var employmentType))
            return Result<Guid>.Failure("Invalid employment type");

        // Create employee
        var employee = Employee.Create(
            request.OrganizationId,
            request.UserId,
            request.EmployeeNumber,
            request.FirstName,
            request.DateOfBirth,
            gender,
            request.DepartmentId,
            request.PositionId,
            employmentType,
            request.HireDate,
            request.LastName,
            maritalStatus,
            request.PersonalEmail,
            request.Phone,
            request.Mobile);

        _context.Set<Employee>().Add(employee);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(employee.Id);
    }
}
