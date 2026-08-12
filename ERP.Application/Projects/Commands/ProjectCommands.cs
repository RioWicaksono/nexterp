using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Application.Projects.DTOs;
using ERP.Domain.Projects.Entities;
using ERP.Domain.Projects.Enums;

namespace ERP.Application.Projects.Commands;

/// <summary>
/// Command to create a project
/// </summary>
public class CreateProjectCommand : ICommand<Guid>
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public Guid? ProjectManagerId { get; set; }
}

/// <summary>
/// Validator for CreateProjectCommand
/// </summary>
public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required")
            .MaximumLength(200).WithMessage("Project name cannot exceed 200 characters");

        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Code cannot exceed 50 characters");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.Budget)
            .GreaterThanOrEqualTo(0).WithMessage("Budget cannot be negative");
    }
}

/// <summary>
/// Handler for CreateProjectCommand
/// </summary>
public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateProjectCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        // Check for duplicate code
        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var existing = await _context.Set<Project>()
                .AnyAsync(p => p.OrganizationId == request.OrganizationId &&
                              p.Code == request.Code.ToUpperInvariant() &&
                              !p.IsDeleted, cancellationToken);

            if (existing)
                return Result<Guid>.Failure("Project code already exists");
        }

        var project = Project.Create(
            request.OrganizationId,
            request.Name,
            request.Code,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.Budget);

        if (request.ProjectManagerId.HasValue)
            project.SetProjectManager(request.ProjectManagerId.Value);

        _context.Set<Project>().Add(project);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(project.Id);
    }
}

/// <summary>
/// Command to create a project task
/// </summary>
public class CreateProjectTaskCommand : ICommand<Guid>
{
    public Guid OrganizationId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? ParentTaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = "Medium";
    public DateTime? DueDate { get; set; }
    public decimal EstimatedHours { get; set; }
    public Guid? AssignedToId { get; set; }
}

/// <summary>
/// Validator for CreateProjectTaskCommand
/// </summary>
public class CreateProjectTaskCommandValidator : AbstractValidator<CreateProjectTaskCommand>
{
    public CreateProjectTaskCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required")
            .MaximumLength(500).WithMessage("Title cannot exceed 500 characters");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority");

        RuleFor(x => x.EstimatedHours)
            .GreaterThanOrEqualTo(0).WithMessage("Estimated hours cannot be negative");
    }
}

/// <summary>
/// Handler for CreateProjectTaskCommand
/// </summary>
public class CreateProjectTaskCommandHandler : IRequestHandler<CreateProjectTaskCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateProjectTaskCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateProjectTaskCommand request, CancellationToken cancellationToken)
    {
        // Check project exists
        var project = await _context.Set<Project>()
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId && !p.IsDeleted, cancellationToken);

        if (project == null)
            return Result<Guid>.Failure("Project not found");

        if (!Enum.TryParse<TaskPriority>(request.Priority, true, out var priority))
            return Result<Guid>.Failure("Invalid priority");

        var task = ProjectTask.Create(
            request.OrganizationId,
            request.ProjectId,
            request.Title,
            request.Description,
            priority,
            request.DueDate,
            request.EstimatedHours);

        if (request.ParentTaskId.HasValue)
            task.SetParent(request.ParentTaskId);

        if (request.AssignedToId.HasValue)
            task.AssignTo(request.AssignedToId.Value);

        project.AddTask(task);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(task.Id);
    }
}

/// <summary>
/// Command to update task status
/// </summary>
public class UpdateTaskStatusCommand : ICommand
{
    public Guid TaskId { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Handler for UpdateTaskStatusCommand
/// </summary>
public class UpdateTaskStatusCommandHandler : IRequestHandler<UpdateTaskStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateTaskStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.Set<ProjectTask>()
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task == null)
            return Result.Failure("Task not found");

        if (!Enum.TryParse<ERP.Domain.Projects.Enums.ProjectTaskStatus>(request.Status, true, out var status))
            return Result.Failure("Invalid status");

        switch (status)
        {
            case ERP.Domain.Projects.Enums.ProjectTaskStatus.InProgress:
                task.Start();
                break;
            case ERP.Domain.Projects.Enums.ProjectTaskStatus.Review:
                task.MoveToReview();
                break;
            case ERP.Domain.Projects.Enums.ProjectTaskStatus.Done:
                task.Complete();
                break;
            case ERP.Domain.Projects.Enums.ProjectTaskStatus.Cancelled:
                task.Cancel();
                break;
            default:
                return Result.Failure("Invalid status transition");
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
