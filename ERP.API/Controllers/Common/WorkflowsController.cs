using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.API.Controllers.Base;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Workflow;
using Asp.Versioning;

namespace ERP.API.Controllers.Common;

/// <summary>
/// Workflow and approval management endpoints.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/workflows")]
[Authorize]
public class WorkflowsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly IWorkflowService _workflowService;

    public WorkflowsController(IMediator mediator, IWorkflowService workflowService)
    {
        _mediator = mediator;
        _workflowService = workflowService;
    }

    /// <summary>
    /// Get pending approvals for current user.
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingApprovals([FromQuery] Guid approverId)
    {
        var result = await _workflowService.GetPendingApprovalsAsync(approverId);
        return Success(result);
    }

    /// <summary>
    /// Get workflow by ID.
    /// </summary>
    [HttpGet("{workflowId}")]
    public async Task<IActionResult> GetWorkflow([FromRoute] Guid workflowId)
    {
        var result = await _workflowService.GetWorkflowAsync(workflowId);
        if (result == null)
            return NotFoundError("Workflow not found");
        return Success(result);
    }

    /// <summary>
    /// Get my workflows (as requester).
    /// </summary>
    [HttpGet("my-requests")]
    public async Task<IActionResult> GetMyWorkflows([FromQuery] Guid requesterId)
    {
        var result = await _workflowService.GetMyWorkflowsAsync(requesterId);
        return Success(result);
    }

    /// <summary>
    /// Get pending approvals summary.
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetPendingSummary([FromQuery] Guid approverId)
    {
        var result = await _workflowService.GetPendingSummaryAsync(approverId);
        return Success(result);
    }

    /// <summary>
    /// Process approval action.
    /// </summary>
    [HttpPost("approve")]
    public async Task<IActionResult> ProcessApproval([FromBody] ProcessApprovalRequest request)
    {
        var result = await _workflowService.ProcessApprovalAsync(request);
        return Success(result);
    }

    /// <summary>
    /// Cancel workflow.
    /// </summary>
    [HttpPost("{workflowId}/cancel")]
    public async Task<IActionResult> CancelWorkflow(
        [FromRoute] Guid workflowId,
        [FromQuery] Guid requesterId,
        [FromQuery] string? reason = null)
    {
        var result = await _workflowService.CancelWorkflowAsync(workflowId, requesterId, reason);
        return Success(result);
    }

    /// <summary>
    /// Get entity approval history.
    /// </summary>
    [HttpGet("entity/{entityId}")]
    public async Task<IActionResult> GetEntityHistory(
        [FromRoute] Guid entityId,
        [FromQuery] string entityType)
    {
        var result = await _workflowService.GetEntityHistoryAsync(entityId, entityType);
        return Success(result);
    }
}
