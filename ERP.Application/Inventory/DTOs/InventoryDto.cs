using ERP.Application.Common.DTOs;

namespace ERP.Application.Inventory.DTOs;

/// <summary>
/// Stock Item data transfer object
/// </summary>
public class StockItemDto : BaseDto
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Barcode { get; set; }
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    public string? UnitOfMeasureName { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal? MinimumStock { get; set; }
    public decimal? MaximumStock { get; set; }
    public decimal? StandardCost { get; set; }
    public decimal? StandardPrice { get; set; }
    public string ValuationMethod { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool TrackSerials { get; set; }
    public bool TrackBatch { get; set; }
    public decimal TotalStock { get; set; }
    public bool IsBelowReorderLevel { get; set; }
}

/// <summary>
/// Warehouse data transfer object
/// </summary>
public class WarehouseDto : BaseDto
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public bool AllowsNegativeStock { get; set; }
}

/// <summary>
/// Stock Transaction data transfer object
/// </summary>
public class StockTransactionDto : BaseDto
{
    public Guid OrganizationId { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public Guid StockItemId { get; set; }
    public string? StockItemName { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public Guid? SourceWarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalAmount { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// DTO for creating a stock item
/// </summary>
public class CreateStockItemDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Barcode { get; set; }
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal? MinimumStock { get; set; }
    public decimal? MaximumStock { get; set; }
    public decimal? StandardCost { get; set; }
    public decimal? StandardPrice { get; set; }
    public string ValuationMethod { get; set; } = "AverageCost";
    public bool TrackSerials { get; set; }
    public bool TrackBatch { get; set; }
}

/// <summary>
/// DTO for creating a warehouse
/// </summary>
public class CreateWarehouseDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsDefault { get; set; }
    public bool AllowsNegativeStock { get; set; }
}
