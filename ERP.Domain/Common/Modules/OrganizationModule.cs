using ERP.Domain.Base;

namespace ERP.Domain.Common.Modules;

/// <summary>
/// Represents a module activation for a specific organization.
/// Tracks when a module was activated and when it expires.
/// </summary>
public class OrganizationModule : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid ModuleId { get; private set; }
    public DateTime ActivatedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public string? ActivatedBy { get; private set; }
    public string? Notes { get; private set; }

    // Navigation properties
    private readonly Domain.Base.Organization _organization = null!;
    public Domain.Base.Organization Organization => _organization;

    private readonly ModuleDefinition _module = null!;
    public ModuleDefinition Module => _module;

    // Required for EF Core
    private OrganizationModule() { }

    public OrganizationModule(Guid organizationId, Guid moduleId, string? activatedBy = null, DateTime? expiresAt = null, string? notes = null)
    {
        OrganizationId = organizationId;
        ModuleId = moduleId;
        ActivatedAt = DateTime.UtcNow;
        ActivatedBy = activatedBy;
        ExpiresAt = expiresAt;
        Notes = notes;
    }

    public bool IsActive => !IsExpired;

    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;

    public void Extend(DateTime newExpiryDate, string extendedBy)
    {
        ExpiresAt = newExpiryDate;
        Notes = $"Extended by {extendedBy} on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";
    }

    public void Revoke(string revokedBy, string reason)
    {
        ExpiresAt = DateTime.UtcNow;
        Notes = $"Revoked by {revokedBy} on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC. Reason: {reason}";
    }
}
