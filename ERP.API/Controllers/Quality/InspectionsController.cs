using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Application.Quality.Commands;
using ERP.API.Controllers.Base;

namespace ERP.API.Controllers.Quality;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InspectionsController : BaseApiController
{
    private readonly IMediator _mediator;

    public InspectionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<InspectionResponse>>> GetAll(
        [FromQuery] string? status,
        [FromQuery] string? type)
    {
        var query = new GetInspectionsQuery(status, type);
        var inspections = await _mediator.Send(query);

        return Ok(inspections.Select(i => new InspectionResponse(
            i.Id, i.OrganizationId, i.InspectionNumber, i.Type,
            i.ReferenceId, i.ReferenceType, i.InspectionDate,
            i.Status, i.Inspector, i.Results, i.Passed, i.Notes, i.CreatedAt
        )));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InspectionResponse>> GetById(Guid id)
    {
        // TODO: Add GetInspectionById query
        return Ok();
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateInspectionRequest request)
    {
        var command = new CreateInspectionCommand(
            request.Type,
            request.ReferenceId,
            request.ReferenceType,
            request.InspectionDate,
            request.Inspector,
            request.Notes
        );

        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id}/complete")]
    public async Task<ActionResult<bool>> Complete(Guid id, [FromBody] CompleteInspectionRequest request)
    {
        var command = new CompleteInspectionCommand(id, request.Results, request.Passed, request.Notes);
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}

public record InspectionResponse(
    Guid Id,
    Guid OrganizationId,
    string InspectionNumber,
    string Type,
    Guid? ReferenceId,
    string ReferenceType,
    DateTime InspectionDate,
    string Status,
    string? Inspector,
    string Results,
    bool Passed,
    string? Notes,
    DateTime CreatedAt
);

public record CreateInspectionRequest(
    string Type,
    Guid? ReferenceId,
    string ReferenceType,
    DateTime InspectionDate,
    string? Inspector,
    string? Notes
);

public record CompleteInspectionRequest(
    string Results,
    bool Passed,
    string? Notes
);
