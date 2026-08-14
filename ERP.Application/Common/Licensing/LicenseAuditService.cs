using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ERP.Application.Common.Licensing;

/// <summary>
/// Interface for license audit logging.
/// </summary>
public interface ILicenseAuditService
{
    /// <summary>
    /// Logs a license validation attempt.
    /// </summary>
    Task LogValidationAttemptAsync(Guid organizationId, string moduleCode, bool success, string? errorMessage = null);

    /// <summary>
    /// Logs a license creation event.
    /// </summary>
    Task LogLicenseCreatedAsync(Guid organizationId, string tier, Guid createdBy);

    /// <summary>
    /// Logs a license update event.
    /// </summary>
    Task LogLicenseUpdatedAsync(Guid organizationId, string oldTier, string newTier, Guid updatedBy);

    /// <summary>
    /// Logs a license expiration event.
    /// </summary>
    Task LogLicenseExpiredAsync(Guid organizationId, string tier);

    /// <summary>
    /// Logs a module access change event.
    /// </summary>
    Task LogModuleAccessChangedAsync(Guid organizationId, string moduleCode, bool enabled, Guid changedBy);

    /// <summary>
    /// Logs a potential tampering detection event.
    /// </summary>
    Task LogTamperingDetectedAsync(Guid organizationId, string details, string? ipAddress = null);

    /// <summary>
    /// Gets audit logs for an organization.
    /// </summary>
    Task<IEnumerable<LicenseAuditEntry>> GetAuditLogsAsync(Guid organizationId, int count = 100);
}

/// <summary>
/// Represents an audit log entry.
/// </summary>
public record LicenseAuditEntry(
    Guid Id,
    Guid? OrganizationId,
    string EventType,
    string Details,
    string? IpAddress,
    Guid? UserId,
    DateTime Timestamp,
    bool IsSuccess);

/// <summary>
/// Types of license audit events.
/// </summary>
public static class LicenseAuditEventTypes
{
    public const string ValidationAttempt = "LICENSE_VALIDATION_ATTEMPT";
    public const string ValidationSuccess = "LICENSE_VALIDATION_SUCCESS";
    public const string ValidationFailed = "LICENSE_VALIDATION_FAILED";
    public const string LicenseCreated = "LICENSE_CREATED";
    public const string LicenseUpdated = "LICENSE_UPDATED";
    public const string LicenseExpired = "LICENSE_EXPIRED";
    public const string LicenseRevoked = "LICENSE_REVOKED";
    public const string ModuleEnabled = "MODULE_ENABLED";
    public const string ModuleDisabled = "MODULE_DISABLED";
    public const string ModuleSync = "MODULE_SYNC";
    public const string TamperingDetected = "TAMPERING_DETECTED";
    public const string IntegrityCheck = "INTEGRITY_CHECK";
}

/// <summary>
/// Service for audit logging of license operations.
/// Uses structured logging for observability.
/// </summary>
public class LicenseAuditService : ILicenseAuditService
{
    private readonly IAuditLogger _auditLogger;

    public LicenseAuditService(IAuditLogger auditLogger)
    {
        _auditLogger = auditLogger;
    }

    public Task LogValidationAttemptAsync(Guid organizationId, string moduleCode, bool success, string? errorMessage = null)
    {
        var eventType = success ? LicenseAuditEventTypes.ValidationSuccess : LicenseAuditEventTypes.ValidationFailed;
        var details = success
            ? $"License validated for module '{moduleCode}'"
            : $"License validation failed for module '{moduleCode}': {errorMessage}";

        return LogEventAsync(organizationId, eventType, details, success);
    }

    public Task LogLicenseCreatedAsync(Guid organizationId, string tier, Guid createdBy)
    {
        var details = $"License created with tier '{tier}'";
        return LogEventAsync(organizationId, LicenseAuditEventTypes.LicenseCreated, details, true, createdBy);
    }

    public Task LogLicenseUpdatedAsync(Guid organizationId, string oldTier, string newTier, Guid updatedBy)
    {
        var details = $"License updated from tier '{oldTier}' to '{newTier}'";
        return LogEventAsync(organizationId, LicenseAuditEventTypes.LicenseUpdated, details, true, updatedBy);
    }

    public Task LogLicenseExpiredAsync(Guid organizationId, string tier)
    {
        var details = $"License expired for tier '{tier}'";
        return LogEventAsync(organizationId, LicenseAuditEventTypes.LicenseExpired, details, false);
    }

    public Task LogModuleAccessChangedAsync(Guid organizationId, string moduleCode, bool enabled, Guid changedBy)
    {
        var eventType = enabled ? LicenseAuditEventTypes.ModuleEnabled : LicenseAuditEventTypes.ModuleDisabled;
        var details = $"Module '{moduleCode}' {(enabled ? "enabled" : "disabled")}";
        return LogEventAsync(organizationId, eventType, details, true, changedBy);
    }

    public Task LogTamperingDetectedAsync(Guid organizationId, string details, string? ipAddress = null)
    {
        return LogEventAsync(organizationId, LicenseAuditEventTypes.TamperingDetected, details, false, null, ipAddress);
    }

    public Task<IEnumerable<LicenseAuditEntry>> GetAuditLogsAsync(Guid organizationId, int count = 100)
    {
        // This would typically query a database or log aggregator
        // For now, return empty list - implementation depends on storage choice
        return Task.FromResult<IEnumerable<LicenseAuditEntry>>(Array.Empty<LicenseAuditEntry>());
    }

    private Task LogEventAsync(
        Guid organizationId,
        string eventType,
        string details,
        bool isSuccess,
        Guid? userId = null,
        string? ipAddress = null)
    {
        var entry = new LicenseAuditEntry(
            Guid.NewGuid(),
            organizationId,
            eventType,
            details,
            ipAddress,
            userId,
            DateTime.UtcNow,
            isSuccess);

        // Use structured logging for observability
        _auditLogger.LogInformation(
            "LicenseAudit: {EventType} for Organization {OrganizationId} - {Details}",
            eventType,
            organizationId,
            details);

        // In production, this would also persist to database/log aggregator
        return Task.CompletedTask;
    }
}

/// <summary>
/// Interface for audit logger (abstraction for logging framework).
/// </summary>
public interface IAuditLogger
{
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(string message, params object[] args);
    void LogError(Exception exception, string message, params object[] args);
}

/// <summary>
/// Serilog-based implementation of IAuditLogger.
/// </summary>
public class SerilogAuditLogger : IAuditLogger
{
    private readonly ILogger _logger;

    public SerilogAuditLogger(ILogger logger)
    {
        _logger = logger;
    }

    public void LogInformation(string message, params object[] args)
    {
        _logger.LogInformation(message, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
    }

    public void LogError(string message, params object[] args)
    {
        _logger.LogError(message, args);
    }

    public void LogError(Exception exception, string message, params object[] args)
    {
        _logger.LogError(exception, message, args);
    }
}
