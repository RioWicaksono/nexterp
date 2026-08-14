namespace ERP.Application.Common.Integrations;

/// <summary>
/// Email/SMS notification gateway interface.
/// </summary>
public interface INotificationGateway
{
    /// <summary>
    /// Send email.
    /// </summary>
    Task<EmailResult> SendEmailAsync(EmailRequest request, CancellationToken ct = default);

    /// <summary>
    /// Send bulk email.
    /// </summary>
    Task<BulkEmailResult> SendBulkEmailAsync(BulkEmailRequest request, CancellationToken ct = default);

    /// <summary>
    /// Send SMS.
    /// </summary>
    Task<SmsResult> SendSmsAsync(SmsRequest request, CancellationToken ct = default);

    /// <summary>
    /// Send bulk SMS.
    /// </summary>
    Task<BulkSmsResult> SendBulkSmsAsync(BulkSmsRequest request, CancellationToken ct = default);

    /// <summary>
    /// Get email delivery status.
    /// </summary>
    Task<EmailDeliveryStatus> GetEmailStatusAsync(string messageId, CancellationToken ct = default);

    /// <summary>
    /// Get SMS delivery status.
    /// </summary>
    Task<SmsDeliveryStatus> GetSmsStatusAsync(string messageId, CancellationToken ct = default);
}

/// <summary>
/// Email request.
/// </summary>
public record EmailRequest(
    string To,
    string Subject,
    string Body,
    string? From = null,
    string? ReplyTo = null,
    List<EmailAttachment>? Attachments = null,
    bool IsHtml = true);

/// <summary>
/// Email attachment.
/// </summary>
public record EmailAttachment(
    string FileName,
    byte[] Content,
    string ContentType);

/// <summary>
/// Email result.
/// </summary>
public record EmailResult(
    bool Success,
    string? MessageId,
    string? ErrorMessage);

/// <summary>
/// Bulk email request.
/// </summary>
public record BulkEmailRequest(
    List<string> Recipients,
    string Subject,
    string Body,
    bool IsHtml = true);

/// <summary>
/// Bulk email result.
/// </summary>
public record BulkEmailResult(
    bool Success,
    int TotalRecipients,
    int SuccessCount,
    int FailedCount,
    List<string> FailedRecipients,
    string? ErrorMessage);

/// <summary>
/// Email delivery status.
/// </summary>
public record EmailDeliveryStatus(
    string MessageId,
    string Status,
    DateTime? DeliveredAt,
    DateTime? OpenedAt,
    int OpenCount);

/// <summary>
/// SMS request.
/// </summary>
public record SmsRequest(
    string To,
    string Message,
    string? SenderId = null);

/// <summary>
/// SMS result.
/// </summary>
public record SmsResult(
    bool Success,
    string? MessageId,
    string? ErrorMessage);

/// <summary>
/// Bulk SMS request.
/// </summary>
public record BulkSmsRequest(
    List<string> Recipients,
    string Message,
    string? SenderId = null);

/// <summary>
/// Bulk SMS result.
/// </summary>
public record BulkSmsResult(
    bool Success,
    int TotalRecipients,
    int SuccessCount,
    int FailedCount,
    List<string> FailedRecipients,
    string? ErrorMessage);

/// <summary>
/// SMS delivery status.
/// </summary>
public record SmsDeliveryStatus(
    string MessageId,
    string Status,
    DateTime? DeliveredAt);
