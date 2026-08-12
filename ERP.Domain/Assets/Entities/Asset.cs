namespace ERP.Domain.Assets.Entities;

/// <summary>
/// Asset entity - Fixed assets like equipment, vehicles, furniture
/// </summary>
public class Asset
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public Guid? ParentAssetId { get; set; }
    public decimal PurchaseCost { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? WarrantyExpiry { get; set; }
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }
}

/// <summary>
/// Asset depreciation record
/// </summary>
public class AssetDepreciation
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal DepreciationAmount { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Asset maintenance record
/// </summary>
public class AssetMaintenance
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string Status { get; set; } = "Scheduled";
    public decimal Cost { get; set; }
    public string? Notes { get; set; }
}
