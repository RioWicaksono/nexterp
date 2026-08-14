using ERP.Domain.Base;

namespace ERP.Domain.Common.Modules;

/// <summary>
/// Defines a license tier with included modules and user limits.
/// </summary>
public class LicenseTier : BaseEntity
{
    public string Code { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int SortOrder { get; private set; }
    public decimal MonthlyPrice { get; private set; }
    public int DefaultMaxUsers { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<OrganizationLicense> _organizationLicenses = new();
    public IReadOnlyCollection<OrganizationLicense> OrganizationLicenses => _organizationLicenses.AsReadOnly();

    // Required for EF Core
    private LicenseTier() { }

    public LicenseTier(string code, string displayName, decimal monthlyPrice, int defaultMaxUsers = 10, string? description = null, int sortOrder = 0)
    {
        Code = code.ToUpperInvariant();
        DisplayName = displayName;
        MonthlyPrice = monthlyPrice;
        DefaultMaxUsers = defaultMaxUsers;
        Description = description;
        SortOrder = sortOrder;
    }

    public void UpdatePricing(decimal monthlyPrice) => MonthlyPrice = monthlyPrice;
    public void UpdateMaxUsers(int maxUsers) => DefaultMaxUsers = maxUsers;
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

/// <summary>
/// Predefined license tier codes
/// </summary>
public static class LicenseTierCodes
{
    public const string Starter = "STARTER";
    public const string Professional = "PROFESSIONAL";
    public const string Enterprise = "ENTERPRISE";
}
