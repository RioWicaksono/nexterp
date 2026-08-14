using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.API.Controllers.Base;
using ERP.Application.Analytics.Commands;
using ERP.Application.Analytics.Queries;
using ERP.Application.Common.Models;
using Asp.Versioning;

namespace ERP.API.Controllers.Analytics;

/// <summary>
/// Notification management endpoints.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationsController : BaseApiController
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get user notifications.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] Guid userId,
        [FromQuery] bool unreadOnly = false)
    {
        var query = new GetUserNotificationsQuery(userId, unreadOnly);
        var result = await _mediator.Send(query);
        return Success(result);
    }

    /// <summary>
    /// Get unread notification count.
    /// </summary>
    [HttpGet("unread/count")]
    public async Task<IActionResult> GetUnreadCount([FromQuery] Guid userId)
    {
        var query = new GetUserNotificationsQuery(userId, true);
        var notifications = await _mediator.Send(query);
        return Success(notifications.Count);
    }

    /// <summary>
    /// Mark notification as read.
    /// </summary>
    [HttpPost("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead([FromRoute] Guid notificationId)
    {
        var command = new MarkNotificationReadCommand(notificationId);
        var result = await _mediator.Send(command);
        return Success(result);
    }

    /// <summary>
    /// Mark all notifications as read.
    /// </summary>
    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead([FromBody] MarkAllReadRequest request)
    {
        var notifications = await _mediator.Send(new GetUserNotificationsQuery(request.UserId, false));
        foreach (var notification in notifications.Where(n => !n.IsRead))
        {
            await _mediator.Send(new MarkNotificationReadCommand(notification.Id));
        }
        return Success(true);
    }

    /// <summary>
    /// Create notification.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationRequest request)
    {
        var command = new CreateNotificationCommand(
            request.UserId,
            request.Type,
            request.Title,
            request.Message,
            request.Link);
        var result = await _mediator.Send(command);
        return Success(result);
    }
}

public record MarkAllReadRequest(Guid UserId);
public record CreateNotificationRequest(
    Guid UserId,
    string Type,
    string Title,
    string Message,
    string? Link = null);
