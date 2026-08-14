using ERP.Domain.Analytics.Entities;

namespace ERP.Application.Analytics.Services;

/// <summary>
/// Notification types enumeration.
/// </summary>
public enum NotificationType
{
    LeaveRequest,
    LeaveApproved,
    LeaveRejected,
    OvertimeRequest,
    OvertimeApproved,
    OvertimeRejected,
    PayrollReady,
    PayrollApproved,
    AttendanceAlert,
    SystemAlert,
    Reminder,
    ApprovalRequired,
    DocumentExpiry,
    Birthday,
    WorkAnniversary
}

/// <summary>
/// Notification priority levels.
/// </summary>
public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Urgent
}

/// <summary>
/// Notification creation request.
/// </summary>
public record CreateNotificationRequest(
    Guid UserId,
    NotificationType Type,
    string Title,
    string Message,
    string? Link = null,
    NotificationPriority Priority = NotificationPriority.Normal);

/// <summary>
/// Notification service interface.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Send a notification to a user.
    /// </summary>
    Task SendNotificationAsync(CreateNotificationRequest request, CancellationToken ct = default);

    /// <summary>
    /// Send notifications to multiple users.
    /// </summary>
    Task SendBulkNotificationsAsync(IEnumerable<CreateNotificationRequest> requests, CancellationToken ct = default);

    /// <summary>
    /// Send approval request notification to approvers.
    /// </summary>
    Task NotifyApprovalRequiredAsync(Guid approverId, string entityType, Guid entityId, string title, CancellationToken ct = default);

    /// <summary>
    /// Send leave request notification.
    /// </summary>
    Task NotifyLeaveRequestAsync(Guid approverId, Guid employeeId, string employeeName, Guid leaveRequestId, CancellationToken ct = default);

    /// <summary>
    /// Send overtime request notification.
    /// </summary>
    Task NotifyOvertimeRequestAsync(Guid approverId, Guid employeeId, string employeeName, Guid overtimeRequestId, CancellationToken ct = default);

    /// <summary>
    /// Send payroll ready notification.
    /// </summary>
    Task NotifyPayrollReadyAsync(IEnumerable<Guid> userIds, int month, int year, CancellationToken ct = default);

    /// <summary>
    /// Mark notification as read.
    /// </summary>
    Task MarkAsReadAsync(Guid notificationId, CancellationToken ct = default);

    /// <summary>
    /// Mark all notifications as read for a user.
    /// </summary>
    Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Delete old notifications.
    /// </summary>
    Task CleanupOldNotificationsAsync(int daysToKeep = 90, CancellationToken ct = default);
}
