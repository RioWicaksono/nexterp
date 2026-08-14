using ERP.Application.Common.Interfaces;
using ERP.Application.Analytics.Services;
using ERP.Domain.Analytics.Entities;

namespace ERP.Infrastructure.Services;

/// <summary>
/// Implementation of notification service.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;

    public NotificationService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SendNotificationAsync(CreateNotificationRequest request, CancellationToken ct = default)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Type = request.Type.ToString(),
            Title = request.Title,
            Message = request.Message,
            Link = request.Link,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(ct);
    }

    public async Task SendBulkNotificationsAsync(IEnumerable<CreateNotificationRequest> requests, CancellationToken ct = default)
    {
        var notifications = requests.Select(r => new Notification
        {
            Id = Guid.NewGuid(),
            UserId = r.UserId,
            Type = r.Type.ToString(),
            Title = r.Title,
            Message = r.Message,
            Link = r.Link,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync(ct);
    }

    public async Task NotifyApprovalRequiredAsync(Guid approverId, string entityType, Guid entityId, string title, CancellationToken ct = default)
    {
        await SendNotificationAsync(new CreateNotificationRequest(
            approverId,
            NotificationType.ApprovalRequired,
            $"Approval Required: {title}",
            $"A new {entityType} requires your approval.",
            $"/{entityType.ToLower()}/{entityId}",
            NotificationPriority.High), ct);
    }

    public async Task NotifyLeaveRequestAsync(Guid approverId, Guid employeeId, string employeeName, Guid leaveRequestId, CancellationToken ct = default)
    {
        await SendNotificationAsync(new CreateNotificationRequest(
            approverId,
            NotificationType.LeaveRequest,
            "New Leave Request",
            $"{employeeName} has submitted a leave request.",
            $"/leave-requests/{leaveRequestId}",
            NotificationPriority.Normal), ct);
    }

    public async Task NotifyOvertimeRequestAsync(Guid approverId, Guid employeeId, string employeeName, Guid overtimeRequestId, CancellationToken ct = default)
    {
        await SendNotificationAsync(new CreateNotificationRequest(
            approverId,
            NotificationType.OvertimeRequest,
            "New Overtime Request",
            $"{employeeName} has submitted an overtime request.",
            $"/overtime-requests/{overtimeRequestId}",
            NotificationPriority.Normal), ct);
    }

    public async Task NotifyPayrollReadyAsync(IEnumerable<Guid> userIds, int month, int year, CancellationToken ct = default)
    {
        var monthName = new DateTime(year, month, 1).ToString("MMMM yyyy");

        var requests = userIds.Select(userId => new CreateNotificationRequest(
            userId,
            NotificationType.PayrollReady,
            "Payroll Ready",
            $"Payroll for {monthName} is now available.",
            $"/payroll",
            NotificationPriority.Normal)).ToList();

        await SendBulkNotificationsAsync(requests, ct);
    }

    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken ct = default)
    {
        var notification = await _context.Notifications.FindAsync(new object[] { notificationId }, ct);
        if (notification != null)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default)
    {
        var unreadNotifications = _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToList();

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task CleanupOldNotificationsAsync(int daysToKeep = 90, CancellationToken ct = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

        var oldNotifications = _context.Notifications
            .Where(n => n.CreatedAt < cutoffDate && n.IsRead)
            .ToList();

        _context.Notifications.RemoveRange(oldNotifications);
        await _context.SaveChangesAsync(ct);
    }
}
