using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.API.Controllers.Base;
using ERP.Application.Accounting.Commands.JournalEntries;
using ERP.Application.Accounting.DTOs;
using ERP.Application.Accounting.Queries;

namespace ERP.API.Controllers.Accounting;

/// <summary>
/// Journal Entry management endpoints
/// </summary>
[ApiController]
[Route("api/v1/journal-entries")]
[Authorize]
public class JournalEntriesController : BaseApiController
{
    private readonly IMediator _mediator;

    public JournalEntriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all journal entries with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<JournalEntryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetJournalEntriesQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get journal entry by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<JournalEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetJournalEntryByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new journal entry
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateJournalEntryDto request, CancellationToken cancellationToken)
    {
        var command = new CreateJournalEntryCommand
        {
            EntryDate = request.EntryDate,
            PostingDate = request.PostingDate,
            Title = request.Title,
            Notes = request.Notes,
            ReferenceId = request.ReferenceId,
            ReferenceType = request.ReferenceType,
            ReferenceNumber = request.ReferenceNumber,
            Lines = request.Lines
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Created($"api/v1/journal-entries/{result.Value}", result);

        return HandleResult(result);
    }

    /// <summary>
    /// Submit journal entry for approval
    /// </summary>
    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SubmitJournalEntryCommand { Id = id }, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Approve journal entry
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ApproveJournalEntryCommand { Id = id }, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Post journal entry (make it final)
    /// </summary>
    [HttpPost("{id:guid}/post")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new PostJournalEntryCommand { Id = id }, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Cancel journal entry
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelJournalEntryCommand { Id = id }, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Reverse journal entry
    /// </summary>
    [HttpPost("{id:guid}/reverse")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reverse(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ReverseJournalEntryCommand { Id = id }, cancellationToken);
        return HandleResult(result);
    }
}
