using ERP.Domain.Base;
using ERP.Domain.Common.Modules;

namespace ERP.Domain.Common.Configuration;

/// <summary>
/// Stores per-organization configuration settings.
/// Used for customization and feature toggles per tenant.
/// </summary>
public class OrganizationSetting : BaseEntity, ITenantEntity
{
    // Setter is needed for EF Core materialization and TenantEntityInterceptor
    // Security is enforced by TenantEntityInterceptor which auto-sets this on insert
    public Guid OrganizationId { get; set; }
    public string SettingKey { get; private set; } = string.Empty;
    public string SettingValue { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsEncrypted { get; private set; }

    // Navigation
    private readonly Domain.Base.Organization _organization = null!;
    public Domain.Base.Organization Organization => _organization;

    // Required for EF Core
    private OrganizationSetting() { }

    public OrganizationSetting(
        Guid organizationId,
        string settingKey,
        string settingValue,
        string category,
        string? description = null,
        bool isEncrypted = false)
    {
        OrganizationId = organizationId;
        SettingKey = settingKey;
        SettingValue = settingValue;
        Category = category;
        Description = description;
        IsEncrypted = isEncrypted;
    }

    public void UpdateValue(string newValue)
    {
        if (IsEncrypted)
            throw new InvalidOperationException("Cannot update encrypted setting directly. Use UpdateEncryptedValue method.");
        SettingValue = newValue;
    }

    public void UpdateEncryptedValue(string encryptedValue) => SettingValue = encryptedValue;
}

/// <summary>
/// Predefined setting keys for common configurations.
/// </summary>
public static class SettingKeys
{
    // HR Settings
    public const string HrOvertimeMaxDaily = "HR.OVERTIME.MAX_DAILY_HOURS";
    public const string HrOvertimeMaxWeekly = "HR.OVERTIME.MAX_WEEKLY_HOURS";
    public const string HrLeaveAnnualDefault = "HR.LEAVE.ANNUAL_DEFAULT_DAYS";
    public const string HrLeaveCarryForwardMax = "HR.LEAVE.CARRY_FORWARD_MAX_DAYS";

    // Inventory Settings
    public const string InvAutoReorder = "INV.AUTO_REORDER";
    public const string InvDefaultWarehouse = "INV.DEFAULT_WAREHOUSE_ID";
    public const string InvLowStockThreshold = "INV.LOW_STOCK_THRESHOLD";

    // Accounting Settings
    public const string AccDefaultTermDays = "ACC.DEFAULT_TERM_DAYS";
    public const string AccTaxRate = "ACC.DEFAULT_TAX_RATE";
    public const string AccCurrency = "ACC.DEFAULT_CURRENCY";

    // General Settings
    public const string GeneralCompanyName = "GENERAL.COMPANY_NAME";
    public const string GeneralTimezone = "GENERAL.TIMEZONE";
    public const string GeneralDateFormat = "GENERAL.DATE_FORMAT";
}

/// <summary>
/// Setting categories for organization settings.
/// </summary>
public static class SettingCategories
{
    public const string General = "GENERAL";
    public const string HR = "HR";
    public const string Inventory = "INVENTORY";
    public const string Accounting = "ACCOUNTING";
    public const string Sales = "SALES";
    public const string Purchasing = "PURCHASING";
}
