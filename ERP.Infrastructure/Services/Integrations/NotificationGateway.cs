using ERP.Application.Common.Integrations;
using Microsoft.Extensions.Logging;

namespace ERP.Infrastructure.Services.Integrations;

/// <summary>
/// Stub implementation of notification gateway.
/// In production, this would integrate with email/SMS providers like SendGrid, Twilio.
/// </summary>
public class NotificationGateway : INotificationGateway
{
    private readonly ILogger<NotificationGateway> _logger;

    public NotificationGateway(ILogger<NotificationGateway> logger)
    {
        _logger = logger;
    }

    public Task<EmailResult> SendEmailAsync(EmailRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Sending email to {To} with subject: {Subject}", request.To, request.Subject);

        // Stub - in production would call SendGrid/AWS SES
        return Task.FromResult(new EmailResult(
            Success: true,
            MessageId: $"email-{Guid.NewGuid():N}",
            ErrorMessage: null));
    }

    public Task<BulkEmailResult> SendBulkEmailAsync(BulkEmailRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Sending bulk email to {Count} recipients with subject: {Subject}",
            request.Recipients.Count, request.Subject);

        return Task.FromResult(new BulkEmailResult(
            Success: true,
            TotalRecipients: request.Recipients.Count,
            SuccessCount: request.Recipients.Count,
            FailedCount: 0,
            FailedRecipients: new List<string>(),
            ErrorMessage: null));
    }

    public Task<SmsResult> SendSmsAsync(SmsRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Sending SMS to {To}: {Message}", request.To, request.Message[..Math.Min(50, request.Message.Length)]);

        // Stub - in production would call Twilio/Nexmo
        return Task.FromResult(new SmsResult(
            Success: true,
            MessageId: $"sms-{Guid.NewGuid():N}",
            ErrorMessage: null));
    }

    public Task<BulkSmsResult> SendBulkSmsAsync(BulkSmsRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Sending bulk SMS to {Count} recipients", request.Recipients.Count);

        return Task.FromResult(new BulkSmsResult(
            Success: true,
            TotalRecipients: request.Recipients.Count,
            SuccessCount: request.Recipients.Count,
            FailedCount: 0,
            FailedRecipients: new List<string>(),
            ErrorMessage: null));
    }

    public Task<EmailDeliveryStatus> GetEmailStatusAsync(string messageId, CancellationToken ct = default)
    {
        _logger.LogInformation("Getting email status for {MessageId}", messageId);

        return Task.FromResult(new EmailDeliveryStatus(
            MessageId: messageId,
            Status: "DELIVERED",
            DeliveredAt: DateTime.UtcNow.AddMinutes(-5),
            OpenedAt: DateTime.UtcNow.AddMinutes(-3),
            OpenCount: 1));
    }

    public Task<SmsDeliveryStatus> GetSmsStatusAsync(string messageId, CancellationToken ct = default)
    {
        _logger.LogInformation("Getting SMS status for {MessageId}", messageId);

        return Task.FromResult(new SmsDeliveryStatus(
            MessageId: messageId,
            Status: "DELIVERED",
            DeliveredAt: DateTime.UtcNow.AddSeconds(-30)));
    }
}
