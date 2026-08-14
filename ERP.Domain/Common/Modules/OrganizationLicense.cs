using ERP.Domain.Base;

namespace ERP.Domain.Common.Modules;

/// <summary>
/// Represents an organization's active license with tier and user limits.
/// </summary>
public class OrganizationLicense : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid LicenseTierId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public int MaxUsers { get; private set; }
    public bool IsAutoRenew { get; private set; }
    public string? BillingEmail { get; private set; }

    // Navigation properties
    private readonly Domain.Base.Organization _organization = null!;
    public Domain.Base.Organization Organization => _organization;

    private readonly LicenseTier _licenseTier = null!;
    public LicenseTier LicenseTier => _licenseTier;

    // Required for EF Core
    private OrganizationLicense() { }

    public OrganizationLicense(
        Guid organizationId,
        Guid licenseTierId,
        DateTime startDate,
        DateTime endDate,
        int maxUsers,
        string? billingEmail = null,
        bool isAutoRenew = false)
    {
        OrganizationId = organizationId;
        LicenseTierId = licenseTierId;
        StartDate = startDate;
        EndDate = endDate;
        MaxUsers = maxUsers;
        BillingEmail = billingEmail;
        IsAutoRenew = isAutoRenew;
    }

    public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;

    public bool IsExpiringSoon(int daysThreshold = 7)
        => IsActive && (EndDate - DateTime.UtcNow).TotalDays <= daysThreshold;

    public bool IsExpired => DateTime.UtcNow > EndDate;

    public void Renew(DateTime newEndDate)
    {
        StartDate = EndDate;
        EndDate = newEndDate;
    }

    public void UpdateMaxUsers(int maxUsers) => MaxUsers = maxUsers;

    public void UpdateBillingEmail(string email) => BillingEmail = email;
}
