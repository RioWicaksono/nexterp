using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Behaviors;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Hrm.Entities;

namespace ERP.Application.Hrm.Commands.Departments;

/// <summary>
/// Command to create a new department
/// </summary>
[RequiresModule("HRM")]
public class CreateDepartmentCommand : ICommand<Guid>
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
}

/// <summary>
/// Validator for CreateDepartmentCommand
/// </summary>
public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty().WithMessage("Organization is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Department name is required")
            .MaximumLength(100).WithMessage("Department name cannot exceed 100 characters");

        RuleFor(x => x.Code)
            .MaximumLength(20).WithMessage("Department code cannot exceed 20 characters")
            .When(x => x.Code != null);
    }
}

/// <summary>
/// Handler for CreateDepartmentCommand
/// </summary>
public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateDepartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        // Check if code already exists
        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var existingCode = await _context.Set<Department>()
                .AnyAsync(d =>
                    d.OrganizationId == request.OrganizationId &&
                    d.Code == request.Code.ToUpperInvariant() &&
                    !d.IsDeleted, cancellationToken);

            if (existingCode)
                return Result<Guid>.Failure("Department code already exists");
        }

        // Check if parent department exists
        if (request.ParentDepartmentId.HasValue)
        {
            var parentExists = await _context.Set<Department>()
                .AnyAsync(d => d.Id == request.ParentDepartmentId.Value && !d.IsDeleted, cancellationToken);

            if (!parentExists)
                return Result<Guid>.Failure("Parent department not found");
        }

        var department = Department.Create(
            request.OrganizationId,
            request.Name,
            request.Code,
            request.Description,
            request.ParentDepartmentId);

        _context.Set<Department>().Add(department);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(department.Id);
    }
}

/// <summary>
/// Command to update a department
/// </summary>
[RequiresModule("HRM")]
public class UpdateDepartmentCommand : ICommand
{
    public Guid DepartmentId { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
}

/// <summary>
/// Validator for UpdateDepartmentCommand
/// </summary>
public class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentCommandValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Department ID is required");

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Department name cannot exceed 100 characters")
            .When(x => x.Name != null);

        RuleFor(x => x.Code)
            .MaximumLength(20).WithMessage("Department code cannot exceed 20 characters")
            .When(x => x.Code != null);
    }
}

/// <summary>
/// Handler for UpdateDepartmentCommand
/// </summary>
public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateDepartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _context.Set<Department>()
            .FirstOrDefaultAsync(d => d.Id == request.DepartmentId && !d.IsDeleted, cancellationToken);

        if (department == null)
            return Result.Failure("Department not found");

        // Check for circular reference
        if (request.ParentDepartmentId.HasValue)
        {
            if (request.ParentDepartmentId.Value == request.DepartmentId)
                return Result.Failure("Department cannot be its own parent");

            // Check if new parent is a descendant
            var isDescendant = await IsDescendantAsync(
                request.DepartmentId,
                request.ParentDepartmentId.Value,
                cancellationToken);

            if (isDescendant)
                return Result.Failure("Cannot set a descendant as parent (circular reference)");
        }

        department.Update(request.Name, request.Code, request.Description);
        department.SetParentDepartment(request.ParentDepartmentId);

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<bool> IsDescendantAsync(Guid parentId, Guid childId, CancellationToken ct)
    {
        var current = await _context.Set<Department>()
            .FirstOrDefaultAsync(d => d.Id == childId && !d.IsDeleted, ct);

        while (current?.ParentDepartmentId != null)
        {
            if (current.ParentDepartmentId == parentId)
                return true;

            current = await _context.Set<Department>()
                .FirstOrDefaultAsync(d => d.Id == current.ParentDepartmentId && !d.IsDeleted, ct);
        }

        return false;
    }
}

/// <summary>
/// Command to create a new position
/// </summary>
[RequiresModule("HRM")]
public class CreatePositionCommand : ICommand<Guid>
{
    public Guid OrganizationId { get; set; }
    public Guid DepartmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Grade { get; set; } = 1;
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
}

/// <summary>
/// Validator for CreatePositionCommand
/// </summary>
public class CreatePositionCommandValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionCommandValidator()
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty().WithMessage("Organization is required");

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Department is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Position title is required")
            .MaximumLength(100).WithMessage("Position title cannot exceed 100 characters");

        RuleFor(x => x.Grade)
            .GreaterThan(0).WithMessage("Grade must be at least 1");

        RuleFor(x => x.MinSalary)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum salary cannot be negative")
            .When(x => x.MinSalary.HasValue);

        RuleFor(x => x.MaxSalary)
            .GreaterThanOrEqualTo(x => x.MinSalary)
            .WithMessage("Maximum salary must be greater than or equal to minimum salary")
            .When(x => x.MinSalary.HasValue && x.MaxSalary.HasValue);
    }
}

/// <summary>
/// Handler for CreatePositionCommand
/// </summary>
public class CreatePositionCommandHandler : IRequestHandler<CreatePositionCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreatePositionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreatePositionCommand request, CancellationToken cancellationToken)
    {
        // Check if department exists
        var departmentExists = await _context.Set<Department>()
            .AnyAsync(d => d.Id == request.DepartmentId && !d.IsDeleted, cancellationToken);

        if (!departmentExists)
            return Result<Guid>.Failure("Department not found");

        var position = Position.Create(
            request.OrganizationId,
            request.DepartmentId,
            request.Title,
            request.Description,
            request.Grade,
            request.MinSalary,
            request.MaxSalary);

        _context.Set<Position>().Add(position);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(position.Id);
    }
}

/// <summary>
/// Command to update a position
/// </summary>
[RequiresModule("HRM")]
public class UpdatePositionCommand : ICommand
{
    public Guid PositionId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? Grade { get; set; }
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
}

/// <summary>
/// Validator for UpdatePositionCommand
/// </summary>
public class UpdatePositionCommandValidator : AbstractValidator<UpdatePositionCommand>
{
    public UpdatePositionCommandValidator()
    {
        RuleFor(x => x.PositionId)
            .NotEmpty().WithMessage("Position ID is required");

        RuleFor(x => x.Title)
            .MaximumLength(100).WithMessage("Position title cannot exceed 100 characters")
            .When(x => x.Title != null);

        RuleFor(x => x.Grade)
            .GreaterThan(0).WithMessage("Grade must be at least 1")
            .When(x => x.Grade.HasValue);
    }
}

/// <summary>
/// Handler for UpdatePositionCommand
/// </summary>
public class UpdatePositionCommandHandler : IRequestHandler<UpdatePositionCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdatePositionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdatePositionCommand request, CancellationToken cancellationToken)
    {
        var position = await _context.Set<Position>()
            .FirstOrDefaultAsync(p => p.Id == request.PositionId && !p.IsDeleted, cancellationToken);

        if (position == null)
            return Result.Failure("Position not found");

        position.Update(request.Title, request.Description, request.Grade);

        if (request.MinSalary.HasValue || request.MaxSalary.HasValue)
        {
            var min = request.MinSalary ?? position.MinSalary;
            var max = request.MaxSalary ?? position.MaxSalary;
            position.SetSalaryRange(min, max);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
