using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Application.Quality.Commands;
using ERP.API.Controllers.Base;

namespace ERP.API.Controllers.Quality;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NonConformancesController : BaseApiController
{
    private readonly IMediator _mediator;

    public NonConformancesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<NonConformanceResponse>>> GetAll(
        [FromQuery] string? status,
        [FromQuery] string? severity)
    {
        var query = new GetNonConformancesQuery(status, severity);
        var items = await _mediator.Send(query);

        return Ok(items.Select(n => new NonConformanceResponse(
            n.Id, n.OrganizationId, n.InspectionId, n.Severity,
            n.Description, n.RootCause, n.CorrectiveAction,
            n.PreventiveAction, n.Status, n.ResolvedAt, n.CreatedAt
        )));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<NonConformanceResponse>> GetById(Guid id)
    {
        // TODO: Add GetNonConformanceById query
        return Ok();
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateNonConformanceRequest request)
    {
        var command = new CreateNonConformanceCommand(
            request.InspectionId,
            request.Severity,
            request.Description,
            request.RootCause,
            request.CorrectiveAction,
            request.PreventiveAction
        );

        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id}/resolve")]
    public async Task<ActionResult<bool>> Resolve(Guid id, [FromBody] ResolveNonConformanceRequest request)
    {
        var command = new ResolveNonConformanceCommand(
            id,
            request.RootCause,
            request.CorrectiveAction,
            request.PreventiveAction
        );

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}

public record NonConformanceResponse(
    Guid Id,
    Guid OrganizationId,
    Guid? InspectionId,
    string Severity,
    string Description,
    string? RootCause,
    string? CorrectiveAction,
    string? PreventiveAction,
    string Status,
    DateTime? ResolvedAt,
    DateTime CreatedAt
);

public record CreateNonConformanceRequest(
    Guid? InspectionId,
    string Severity,
    string Description,
    string? RootCause,
    string? CorrectiveAction,
    string? PreventiveAction
);

public record ResolveNonConformanceRequest(
    string? RootCause,
    string? CorrectiveAction,
    string? PreventiveAction
);
