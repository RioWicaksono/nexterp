using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Behaviors;
using ERP.Domain.Common.Workflow;

namespace ERP.Application.Common.Commands;

/// <summary>
/// Command to submit an entity for approval
/// </summary>
[RequiresPermission(PermissionRequirementType.RequireAny, "admin.approval.submit", "purchasing.orders.submit", "sales.orders.submit")]
public class SubmitForApprovalCommand : ICommand<ApprovalResult>
{
    public WorkflowType WorkflowType { get; set; }
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? Amount { get; set; }
}

/// <summary>
/// Result of approval operation
/// </summary>
public class ApprovalResult
{
    public bool Success { get; set; }
    public Guid? WorkflowInstanceId { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Validator for SubmitForApprovalCommand
/// </summary>
public class SubmitForApprovalCommandValidator : AbstractValidator<SubmitForApprovalCommand>
{
    public SubmitForApprovalCommandValidator()
    {
        RuleFor(x => x.WorkflowType)
            .IsInEnum().WithMessage("Invalid workflow type");

        RuleFor(x => x.EntityId)
            .NotEmpty().WithMessage("Entity ID is required");

        RuleFor(x => x.EntityType)
            .NotEmpty().WithMessage("Entity type is required")
            .MaximumLength(100);

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(255);
    }
}

/// <summary>
/// Command to approve a workflow item
/// </summary>
[RequiresPermission(PermissionRequirementType.RequireAny, "admin.approval.approve", "purchasing.orders.approve", "sales.orders.approve")]
public class ApproveWorkflowCommand : ICommand<ApprovalResult>
{
    public Guid WorkflowInstanceId { get; set; }
    public string? Comments { get; set; }
}

/// <summary>
/// Validator for ApproveWorkflowCommand
/// </summary>
public class ApproveWorkflowCommandValidator : AbstractValidator<ApproveWorkflowCommand>
{
    public ApproveWorkflowCommandValidator()
    {
        RuleFor(x => x.WorkflowInstanceId)
            .NotEmpty().WithMessage("Workflow instance ID is required");
    }
}

/// <summary>
/// Command to reject a workflow item
/// </summary>
[RequiresPermission(PermissionRequirementType.RequireAny, "admin.approval.reject", "purchasing.orders.reject", "sales.orders.reject")]
public class RejectWorkflowCommand : ICommand<ApprovalResult>
{
    public Guid WorkflowInstanceId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Validator for RejectWorkflowCommand
/// </summary>
public class RejectWorkflowCommandValidator : AbstractValidator<RejectWorkflowCommand>
{
    public RejectWorkflowCommandValidator()
    {
        RuleFor(x => x.WorkflowInstanceId)
            .NotEmpty().WithMessage("Workflow instance ID is required");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Rejection reason is required")
            .MaximumLength(500);
    }
}

/// <summary>
/// Command to return a workflow item to requester
/// </summary>
[RequiresPermission(PermissionRequirementType.RequireAny, "admin.approval.return", "purchasing.orders.return", "sales.orders.return")]
public class ReturnWorkflowCommand : ICommand<ApprovalResult>
{
    public Guid WorkflowInstanceId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Query to get pending approvals
/// </summary>
[RequiresPermission(PermissionRequirementType.RequireAny, "admin.approval.read", "purchasing.orders.read", "sales.orders.read")]
public class GetPendingApprovalsQuery : IQuery<List<PendingApprovalDto>>
{
    public WorkflowType? WorkflowType { get; set; }
    public int PageSize { get; set; } = 20;
    public int PageNumber { get; set; } = 1;
}

/// <summary>
/// DTO for pending approval
/// </summary>
public class PendingApprovalDto
{
    public Guid Id { get; set; }
    public WorkflowType Type { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public WorkflowStatus Status { get; set; }
    public int CurrentLevel { get; set; }
    public int TotalLevels { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public string CurrentApproverName { get; set; } = string.Empty;
}

/// <summary>
/// Handler for workflow commands
/// </summary>
public class WorkflowCommandHandler :
    IRequestHandler<SubmitForApprovalCommand, Result<ApprovalResult>>,
    IRequestHandler<ApproveWorkflowCommand, Result<ApprovalResult>>,
    IRequestHandler<RejectWorkflowCommand, Result<ApprovalResult>>,
    IRequestHandler<ReturnWorkflowCommand, Result<ApprovalResult>>,
    IRequestHandler<GetPendingApprovalsQuery, Result<List<PendingApprovalDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IWorkflowService _workflowService;
    private readonly ILogger<WorkflowCommandHandler> _logger;

    public WorkflowCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IWorkflowService workflowService,
        ILogger<WorkflowCommandHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _workflowService = workflowService;
        _logger = logger;
    }

    public async Task<Result<ApprovalResult>> Handle(SubmitForApprovalCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Create workflow instance
            var workflow = new WorkflowInstance
            {
                OrganizationId = _currentUser.OrganizationId ?? throw new InvalidOperationException("No organization context"),
                Type = request.WorkflowType,
                EntityId = request.EntityId,
                EntityType = request.EntityType,
                Title = request.Title,
                Description = request.Description,
                RequesterId = _currentUser.UserId ?? Guid.Empty,
                RequesterName = _currentUser.Username ?? "Unknown",
                Status = WorkflowStatus.Pending,
                CurrentLevel = 1,
                TotalLevels = 1
            };

            _context.Set<WorkflowInstance>().Add(workflow);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Workflow {WorkflowId} created for {EntityType} {EntityId} by user {UserId}",
                workflow.Id, request.EntityType, request.EntityId, _currentUser.UserId);

            return Result<ApprovalResult>.Success(new ApprovalResult
            {
                Success = true,
                WorkflowInstanceId = workflow.Id,
                Message = "Workflow submitted for approval"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit workflow for approval");
            return Result<ApprovalResult>.Failure($"Failed to submit for approval: {ex.Message}");
        }
    }

    public async Task<Result<ApprovalResult>> Handle(ApproveWorkflowCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var workflow = await _context.Set<WorkflowInstance>()
                .FirstOrDefaultAsync(w => w.Id == request.WorkflowInstanceId, cancellationToken);

            if (workflow == null)
                return Result<ApprovalResult>.Failure("Workflow not found");

            if (workflow.Status != WorkflowStatus.Pending)
                return Result<ApprovalResult>.Failure($"Cannot approve workflow in {workflow.Status} status");

            workflow.Status = WorkflowStatus.Approved;
            workflow.CompletedDate = DateTime.UtcNow;
            workflow.CompletedBy = _currentUser.Username;
            workflow.Notes = request.Comments;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Workflow {WorkflowId} approved by {UserId}",
                workflow.Id, _currentUser.UserId);

            return Result<ApprovalResult>.Success(new ApprovalResult
            {
                Success = true,
                WorkflowInstanceId = workflow.Id,
                Message = "Workflow approved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to approve workflow");
            return Result<ApprovalResult>.Failure($"Failed to approve: {ex.Message}");
        }
    }

    public async Task<Result<ApprovalResult>> Handle(RejectWorkflowCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var workflow = await _context.Set<WorkflowInstance>()
                .FirstOrDefaultAsync(w => w.Id == request.WorkflowInstanceId, cancellationToken);

            if (workflow == null)
                return Result<ApprovalResult>.Failure("Workflow not found");

            if (workflow.Status != WorkflowStatus.Pending)
                return Result<ApprovalResult>.Failure($"Cannot reject workflow in {workflow.Status} status");

            workflow.Status = WorkflowStatus.Rejected;
            workflow.CompletedDate = DateTime.UtcNow;
            workflow.CompletedBy = _currentUser.Username;
            workflow.Notes = request.Reason;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Workflow {WorkflowId} rejected by {UserId}: {Reason}",
                workflow.Id, _currentUser.UserId, request.Reason);

            return Result<ApprovalResult>.Success(new ApprovalResult
            {
                Success = true,
                WorkflowInstanceId = workflow.Id,
                Message = "Workflow rejected"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reject workflow");
            return Result<ApprovalResult>.Failure($"Failed to reject: {ex.Message}");
        }
    }

    public async Task<Result<ApprovalResult>> Handle(ReturnWorkflowCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var workflow = await _context.Set<WorkflowInstance>()
                .FirstOrDefaultAsync(w => w.Id == request.WorkflowInstanceId, cancellationToken);

            if (workflow == null)
                return Result<ApprovalResult>.Failure("Workflow not found");

            if (workflow.Status != WorkflowStatus.Pending)
                return Result<ApprovalResult>.Failure($"Cannot return workflow in {workflow.Status} status");

            workflow.Status = WorkflowStatus.Returned;
            workflow.Notes = request.Reason;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Workflow {WorkflowId} returned to requester by {UserId}: {Reason}",
                workflow.Id, _currentUser.UserId, request.Reason);

            return Result<ApprovalResult>.Success(new ApprovalResult
            {
                Success = true,
                WorkflowInstanceId = workflow.Id,
                Message = "Workflow returned to requester"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to return workflow");
            return Result<ApprovalResult>.Failure($"Failed to return: {ex.Message}");
        }
    }

    public async Task<Result<List<PendingApprovalDto>>> Handle(GetPendingApprovalsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var orgId = _currentUser.OrganizationId ?? Guid.Empty;

            var query = _context.Set<WorkflowInstance>()
                .Where(w => w.OrganizationId == orgId && w.Status == WorkflowStatus.Pending);

            if (request.WorkflowType.HasValue)
            {
                query = query.Where(w => w.Type == request.WorkflowType.Value);
            }

            var workflows = await query
                .OrderByDescending(w => w.RequestDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(w => new PendingApprovalDto
                {
                    Id = w.Id,
                    Type = w.Type,
                    EntityType = w.EntityType,
                    EntityId = w.EntityId,
                    Title = w.Title,
                    Description = w.Description,
                    Status = w.Status,
                    CurrentLevel = w.CurrentLevel,
                    TotalLevels = w.TotalLevels,
                    RequesterName = w.RequesterName,
                    RequestDate = w.RequestDate,
                    CurrentApproverName = ""
                })
                .ToListAsync(cancellationToken);

            return Result<List<PendingApprovalDto>>.Success(workflows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get pending approvals");
            return Result<List<PendingApprovalDto>>.Failure($"Failed to get pending approvals: {ex.Message}");
        }
    }
}
