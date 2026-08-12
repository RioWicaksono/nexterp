using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Application.Analytics.Commands;
using ERP.API.Controllers.Base;

namespace ERP.API.Controllers.Analytics;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : BaseApiController
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateNotificationRequest request)
    {
        var command = new CreateNotificationCommand(
            request.UserId,
            request.Type,
            request.Title,
            request.Message,
            request.Link
        );

        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(Create), new { id });
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<NotificationResponse>>> GetUserNotifications(
        Guid userId,
        [FromQuery] bool unreadOnly = false)
    {
        var query = new GetUserNotificationsQuery(userId, unreadOnly);
        var notifications = await _mediator.Send(query);

        return Ok(notifications.Select(n => new NotificationResponse(
            n.Id, n.UserId, n.Type, n.Title, n.Message,
            n.Link, n.IsRead, n.ReadAt, n.CreatedAt
        )));
    }

    [HttpPut("{id}/read")]
    public async Task<ActionResult<bool>> MarkAsRead(Guid id)
    {
        var result = await _mediator.Send(new MarkNotificationReadCommand(id));
        return Ok(result);
    }
}

public record CreateNotificationRequest(
    Guid UserId,
    string Type,
    string Title,
    string Message,
    string? Link
);

public record NotificationResponse(
    Guid Id,
    Guid UserId,
    string Type,
    string Title,
    string Message,
    string? Link,
    bool IsRead,
    DateTime? ReadAt,
    DateTime CreatedAt
);
