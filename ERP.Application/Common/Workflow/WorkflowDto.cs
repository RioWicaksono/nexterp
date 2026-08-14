using ERP.Domain.Common.Workflow;

namespace ERP.Application.Common.Workflow;

/// <summary>
/// Request to start a new workflow.
/// </summary>
public record StartWorkflowRequest(
    Guid OrganizationId,
    WorkflowType Type,
    Guid EntityId,
    string EntityType,
    Guid RequesterId,
    string RequesterName,
    string Title,
    string Description,
    Dictionary<string, object>? Metadata = null);

/// <summary>
/// Request to process approval action.
/// </summary>
public record ProcessApprovalRequest(
    Guid WorkflowApprovalId,
    Guid ApproverId,
    WorkflowStatus Action,  // Approved, Rejected, Returned
    string? Comments = null);

/// <summary>
/// Workflow summary DTO.
/// </summary>
public record WorkflowSummaryDto
{
    public Guid Id { get; init; }
    public WorkflowType Type { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public Guid EntityId { get; init; }
    public WorkflowStatus Status { get; init; }
    public string Title { get; init; } = string.Empty;
    public string RequesterName { get; init; } = string.Empty;
    public DateTime RequestDate { get; init; }
    public int CurrentLevel { get; init; }
    public int TotalLevels { get; init; }
    public string StatusDisplay { get; init; } = string.Empty;
}

/// <summary>
/// Workflow detail DTO.
/// </summary>
public record WorkflowDetailDto
{
    public Guid Id { get; init; }
    public WorkflowType Type { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public Guid EntityId { get; init; }
    public WorkflowStatus Status { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid RequesterId { get; init; }
    public string RequesterName { get; init; } = string.Empty;
    public DateTime RequestDate { get; init; }
    public DateTime? CompletedDate { get; init; }
    public string? CompletedBy { get; init; }
    public string? Notes { get; init; }
    public List<ApprovalStepDto> Approvals { get; init; } = new();
}

/// <summary>
/// Approval step DTO.
/// </summary>
public record ApprovalStepDto
{
    public Guid Id { get; init; }
    public int Level { get; init; }
    public string LevelName { get; init; } = string.Empty;
    public Guid ApproverId { get; init; }
    public string ApproverName { get; init; } = string.Empty;
    public WorkflowStatus Status { get; init; }
    public DateTime? ApprovedAt { get; init; }
    public DateTime? RejectedAt { get; init; }
    public string? Comments { get; init; }
    public bool IsCurrentStep { get; init; }
}

/// <summary>
/// Pending approvals summary.
/// </summary>
public record PendingApprovalsSummaryDto
{
    public int TotalPending { get; init; }
    public int LeaveRequests { get; init; }
    public int OvertimeRequests { get; init; }
    public int OtherRequests { get; init; }
    public List<WorkflowSummaryDto> RecentPending { get; init; } = new();
}
