using ERP.Domain.Common;
using ERP.Domain.Inventory.Enums;

namespace ERP.Domain.Inventory.Entities;

/// <summary>
/// Unit of Measure entity
/// </summary>
public class UnitOfMeasure : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string ShortName { get; private set; } = string.Empty;
    public string? Abbreviation { get; private set; }
    public UomType Type { get; private set; }
    public decimal? FactorToBase { get; private set; }  // Conversion factor to base UOM
    public Guid? BaseUomId { get; private set; }       // Reference to base UOM
    public bool IsActive { get; private set; } = true;

    // Navigation properties
    private readonly UnitOfMeasure? _baseUom;
    public UnitOfMeasure? BaseUom => _baseUom;

    // Factory method
    public static UnitOfMeasure Create(
        Guid organizationId,
        string name,
        string shortName,
        UomType type,
        string? abbreviation = null,
        decimal? factorToBase = null,
        Guid? baseUomId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("UOM name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(shortName))
            throw new ArgumentException("UOM short name is required", nameof(shortName));

        return new UnitOfMeasure
        {
            OrganizationId = organizationId,
            Name = name.Trim(),
            ShortName = shortName.Trim(),
            Abbreviation = abbreviation?.Trim(),
            Type = type,
            FactorToBase = factorToBase,
            BaseUomId = baseUomId
        };
    }

    public void Update(string? name = null, string? shortName = null, string? abbreviation = null)
    {
        Name = name?.Trim() ?? Name;
        ShortName = shortName?.Trim() ?? ShortName;
        Abbreviation = abbreviation?.Trim() ?? Abbreviation;
        UpdateTimestamp();
    }

    public void Activate() { IsActive = true; UpdateTimestamp(); }
    public void Deactivate() { IsActive = false; UpdateTimestamp(); }
}

/// <summary>
/// Stock Item entity representing an inventory product
/// </summary>
public class StockItem : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; }
    public string? Barcode { get; private set; }
    public string? Description { get; private set; }
    public Guid? CategoryId { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }
    public decimal ReorderLevel { get; private set; }
    public decimal? MinimumStock { get; private set; }
    public decimal? MaximumStock { get; private set; }
    public decimal? StandardCost { get; private set; }
    public decimal? StandardPrice { get; private set; }
    public ValuationMethod ValuationMethod { get; private set; } = ValuationMethod.AverageCost;
    public bool IsActive { get; private set; } = true;
    public bool TrackSerials { get; private set; }
    public bool TrackBatch { get; private set; }
    public DateTime? ExpiryDays { get; private set; }
    public decimal Weight { get; private set; }
    public decimal? Length { get; private set; }
    public decimal? Width { get; private set; }
    public decimal? Height { get; private set; }

    // Navigation properties
    private readonly UnitOfMeasure? _unitOfMeasure;
    public UnitOfMeasure? UnitOfMeasure => _unitOfMeasure;

    private readonly List<StockItemWarehouse> _warehouses = new();
    public IReadOnlyCollection<StockItemWarehouse> Warehouses => _warehouses.AsReadOnly();

    // Calculated properties (from domain logic)
    public decimal TotalStock => _warehouses.Sum(w => w.Quantity);
    public bool IsBelowReorderLevel => TotalStock < ReorderLevel;
    public bool IsBelowMinimum => MinimumStock.HasValue && TotalStock < MinimumStock.Value;

    // Factory method
    public static StockItem Create(
        Guid organizationId,
        string name,
        Guid unitOfMeasureId,
        string? code = null,
        string? barcode = null,
        string? description = null,
        Guid? categoryId = null,
        ValuationMethod valuationMethod = ValuationMethod.AverageCost,
        decimal reorderLevel = 0,
        decimal? minimumStock = null,
        decimal? maximumStock = null,
        decimal? standardCost = null,
        decimal? standardPrice = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Item name is required", nameof(name));

        return new StockItem
        {
            OrganizationId = organizationId,
            Name = name.Trim(),
            Code = code?.Trim().ToUpperInvariant(),
            Barcode = barcode?.Trim(),
            Description = description?.Trim(),
            CategoryId = categoryId,
            UnitOfMeasureId = unitOfMeasureId,
            ValuationMethod = valuationMethod,
            ReorderLevel = reorderLevel,
            MinimumStock = minimumStock,
            MaximumStock = maximumStock,
            StandardCost = standardCost,
            StandardPrice = standardPrice
        };
    }

    public void Update(
        string? name = null,
        string? code = null,
        string? barcode = null,
        string? description = null,
        Guid? categoryId = null,
        decimal? reorderLevel = null,
        decimal? minimumStock = null,
        decimal? maximumStock = null,
        decimal? standardCost = null,
        decimal? standardPrice = null)
    {
        Name = name?.Trim() ?? Name;
        Code = code?.Trim().ToUpperInvariant() ?? Code;
        Barcode = barcode?.Trim() ?? Barcode;
        Description = description?.Trim() ?? Description;
        CategoryId = categoryId ?? CategoryId;
        ReorderLevel = reorderLevel ?? ReorderLevel;
        MinimumStock = minimumStock ?? MinimumStock;
        MaximumStock = maximumStock ?? MaximumStock;
        StandardCost = standardCost ?? StandardCost;
        StandardPrice = standardPrice ?? StandardPrice;
        UpdateTimestamp();
    }

    public void SetValuationMethod(ValuationMethod method)
    {
        ValuationMethod = method;
        UpdateTimestamp();
    }

    public void EnableSerialTracking() { TrackSerials = true; UpdateTimestamp(); }
    public void DisableSerialTracking() { TrackSerials = false; UpdateTimestamp(); }
    public void EnableBatchTracking() { TrackBatch = true; UpdateTimestamp(); }
    public void DisableBatchTracking() { TrackBatch = false; UpdateTimestamp(); }

    public void Activate() { IsActive = true; UpdateTimestamp(); }
    public void Deactivate() { IsActive = false; UpdateTimestamp(); }

    public void AddWarehouse(StockItemWarehouse itemWarehouse)
    {
        if (!_warehouses.Any(w => w.WarehouseId == itemWarehouse.WarehouseId))
        {
            _warehouses.Add(itemWarehouse);
            UpdateTimestamp();
        }
    }

    public void UpdateStockLevels(decimal quantity, decimal averageCost)
    {
        StandardCost = averageCost;
        UpdateTimestamp();
    }
}

/// <summary>
/// Stock item quantity in a specific warehouse
/// </summary>
public class StockItemWarehouse : BaseEntity
{
    public Guid StockItemId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal AverageCost { get; private set; }
    public decimal? ReservedQuantity { get; private set; }
    public DateTime? LastStockIn { get; private set; }
    public DateTime? LastStockOut { get; private set; }

    // Navigation properties
    private readonly StockItem? _stockItem;
    public StockItem? StockItem => _stockItem;

    private readonly Warehouse? _warehouse;
    public Warehouse? Warehouse => _warehouse;

    // Available quantity (not reserved)
    public decimal AvailableQuantity => Quantity - (ReservedQuantity ?? 0);

    public static StockItemWarehouse Create(Guid stockItemId, Guid warehouseId) => new()
    {
        StockItemId = stockItemId,
        WarehouseId = warehouseId,
        Quantity = 0,
        AverageCost = 0
    };

    public void AddStock(decimal quantity, decimal cost)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        // Calculate new average cost
        var totalValue = (Quantity * AverageCost) + (quantity * cost);
        Quantity += quantity;
        AverageCost = Quantity > 0 ? Math.Round(totalValue / Quantity, 4) : cost;
        LastStockIn = DateTime.UtcNow;
    }

    public void RemoveStock(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (quantity > AvailableQuantity)
            throw new InvalidOperationException("Insufficient stock");

        Quantity -= quantity;
        LastStockOut = DateTime.UtcNow;
    }

    public void Reserve(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (quantity > AvailableQuantity)
            throw new InvalidOperationException("Insufficient available stock for reservation");

        ReservedQuantity = (ReservedQuantity ?? 0) + quantity;
    }

    public void Unreserve(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (quantity > ReservedQuantity)
            throw new InvalidOperationException("Cannot unreserve more than reserved");

        ReservedQuantity -= quantity;
    }

    public void SetInitialStock(decimal quantity, decimal cost)
    {
        Quantity = quantity;
        AverageCost = cost;
        LastStockIn = DateTime.UtcNow;
    }
}
