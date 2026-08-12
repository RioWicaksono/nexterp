using ERP.Application.Common.Interfaces;
using ERP.Domain.Analytics.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Analytics.Commands;

public record CreateAuditLogCommand(
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
) : IRequest<Guid>;

public class CreateAuditLogHandler : IRequestHandler<CreateAuditLogCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateAuditLogHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateAuditLogCommand request, CancellationToken cancellationToken)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            OrganizationId = request.OrganizationId,
            UserId = request.UserId,
            Module = request.Module,
            Action = request.Action,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            Description = request.Description,
            OldValues = request.OldValues,
            NewValues = request.NewValues,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            CreatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);

        return auditLog.Id;
    }
}

public record CreateNotificationCommand(
    Guid UserId,
    string Type,
    string Title,
    string Message,
    string? Link
) : IRequest<Guid>;

public class CreateNotificationHandler : IRequestHandler<CreateNotificationCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateNotificationHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Type = request.Type,
            Title = request.Title,
            Message = request.Message,
            Link = request.Link,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);

        return notification.Id;
    }
}

public record MarkNotificationReadCommand(Guid NotificationId) : IRequest<bool>;

public class MarkNotificationReadHandler : IRequestHandler<MarkNotificationReadCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public MarkNotificationReadHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId, cancellationToken);

        if (notification == null) return false;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record GetUserNotificationsQuery(Guid UserId, bool UnreadOnly = false) : IRequest<List<Notification>>;

public class GetUserNotificationsHandler : IRequestHandler<GetUserNotificationsQuery, List<Notification>>
{
    private readonly IApplicationDbContext _context;

    public GetUserNotificationsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Notification>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Notifications.Where(n => n.UserId == request.UserId);

        if (request.UnreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);
    }
}
