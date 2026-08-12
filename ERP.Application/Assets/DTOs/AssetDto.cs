using ERP.Domain.Assets.Entities;

namespace ERP.Application.Assets.DTOs;

public class AssetDto
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

public class AssetDepreciationDto
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal DepreciationAmount { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AssetMaintenanceDto
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

// Command DTOs
public record CreateAssetCommand(
    string AssetCode,
    string Name,
    string AssetType,
    decimal PurchaseCost,
    DateTime? PurchaseDate,
    DateTime? WarrantyExpiry,
    string? Notes);

public record CreateAssetDepreciationCommand(
    Guid AssetId,
    int Year,
    int Month,
    decimal DepreciationAmount,
    decimal AccumulatedDepreciation);

public record CreateAssetMaintenanceCommand(
    Guid AssetId,
    string Type,
    DateTime ScheduledDate,
    decimal Cost,
    string? Notes);

public record UpdateAssetCommand(
    Guid Id,
    string Name,
    string AssetType,
    string Status,
    string? Notes);
