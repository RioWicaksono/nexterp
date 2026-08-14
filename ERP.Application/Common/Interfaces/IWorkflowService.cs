using ERP.Application.Common.Workflow;
using ERP.Domain.Common.Workflow;

namespace ERP.Application.Common.Interfaces;

/// <summary>
/// Workflow service interface for managing approval workflows.
/// </summary>
public interface IWorkflowService
{
    /// <summary>
    /// Start a new workflow instance.
    /// </summary>
    Task<Guid> StartWorkflowAsync(StartWorkflowRequest request, CancellationToken ct = default);

    /// <summary>
    /// Get pending approvals for an approver.
    /// </summary>
    Task<List<WorkflowSummaryDto>> GetPendingApprovalsAsync(Guid approverId, CancellationToken ct = default);

    /// <summary>
    /// Get workflow by ID.
    /// </summary>
    Task<WorkflowDetailDto?> GetWorkflowAsync(Guid workflowId, CancellationToken ct = default);

    /// <summary>
    /// Get workflows for a requester.
    /// </summary>
    Task<List<WorkflowSummaryDto>> GetMyWorkflowsAsync(Guid requesterId, CancellationToken ct = default);

    /// <summary>
    /// Process approval action.
    /// </summary>
    Task<bool> ProcessApprovalAsync(ProcessApprovalRequest request, CancellationToken ct = default);

    /// <summary>
    /// Cancel a workflow (only by requester).
    /// </summary>
    Task<bool> CancelWorkflowAsync(Guid workflowId, Guid requesterId, string? reason = null, CancellationToken ct = default);

    /// <summary>
    /// Get pending approvals summary.
    /// </summary>
    Task<PendingApprovalsSummaryDto> GetPendingSummaryAsync(Guid approverId, CancellationToken ct = default);

    /// <summary>
    /// Get approval history for an entity.
    /// </summary>
    Task<List<WorkflowDetailDto>> GetEntityHistoryAsync(Guid entityId, string entityType, CancellationToken ct = default);
}
