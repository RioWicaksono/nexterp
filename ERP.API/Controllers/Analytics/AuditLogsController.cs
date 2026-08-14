using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

using ERP.Application.Analytics.Commands;
using ERP.API.Controllers.Base;

namespace ERP.API.Controllers.Analytics;

[ApiVersion("1.0")]
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditLogsController : BaseApiController
{
    private readonly IMediator _mediator;

    public AuditLogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateAuditLogRequest request)
    {
        var command = new CreateAuditLogCommand(
            request.OrganizationId,
            request.UserId,
            request.Module,
            request.Action,
            request.EntityType,
            request.EntityId,
            request.Description,
            request.OldValues,
            request.NewValues,
            request.IpAddress,
            request.UserAgent
        );

        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(Create), new { id });
    }
}

public record CreateAuditLogRequest(
    Guid OrganizationId,
    Guid? UserId,
    string Module,
    string Action,
    string EntityType,
    Guid? EntityId,
    string Description,
    string? OldValues,
    string? NewValues,
    string IpAddress,
    string? UserAgent
);
