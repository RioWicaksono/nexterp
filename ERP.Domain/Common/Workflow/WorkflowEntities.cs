namespace ERP.Domain.Common.Workflow;

/// <summary>
/// Workflow status enumeration.
/// </summary>
public enum WorkflowStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled,
    Returned
}

/// <summary>
/// Approval level enumeration.
/// </summary>
public enum ApprovalLevel
{
    FirstLevel,
    SecondLevel,
    FinalLevel
}

/// <summary>
/// Workflow type enumeration.
/// </summary>
public enum WorkflowType
{
    LeaveRequest,
    OvertimeRequest,
    ExpenseClaim,
    PurchaseRequest,
    PayrollApproval,
    AssetRequest,
    DocumentApproval
}

/// <summary>
/// Base workflow entity for approval chain.
/// </summary>
public class WorkflowInstance
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public WorkflowType Type { get; set; }
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Pending;
    public int CurrentLevel { get; set; } = 1;
    public int TotalLevels { get; set; } = 1;
    public Guid RequesterId { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CompletedBy { get; set; }
    public string? Notes { get; set; }

    public List<WorkflowApproval> Approvals { get; set; } = new();
}

/// <summary>
/// Individual approval step in the workflow.
/// </summary>
public class WorkflowApproval
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkflowInstanceId { get; set; }
    public int Level { get; set; }
    public ApprovalLevel ApprovalLevel { get; set; }
    public Guid ApproverId { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Pending;
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public string? Comments { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public WorkflowInstance? WorkflowInstance { get; set; }
}

/// <summary>
/// Workflow definition for configuring approval chains.
/// </summary>
public class WorkflowDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public WorkflowType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalLevels { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public List<WorkflowDefinitionLevel> Levels { get; set; } = new();
}

/// <summary>
/// Level configuration for workflow definition.
/// </summary>
public class WorkflowDefinitionLevel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkflowDefinitionId { get; set; }
    public int Level { get; set; }
    public ApprovalLevel ApprovalLevel { get; set; }
    public string ApproverRole { get; set; } = string.Empty;
    public Guid? SpecificApproverId { get; set; }
    public bool CanDelegate { get; set; } = false;
    public int? MaxDelegationDays { get; set; }
}
