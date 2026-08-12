using ERP.Domain.Common;
using ERP.Domain.Inventory.Enums;

namespace ERP.Domain.Inventory.Entities;

/// <summary>
/// Stock Transaction entity for tracking all inventory movements
/// </summary>
public class StockTransaction : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string TransactionNumber { get; private set; } = string.Empty;
    public StockTransactionType Type { get; private set; }
    public DateTime TransactionDate { get; private set; }
    public Guid StockItemId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid? SourceWarehouseId { get; private set; }  // For transfers
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal TotalAmount { get; private set; }
    public Guid? ReferenceId { get; private set; }        // Links to Sales Order, Purchase Order, etc.
    public string? ReferenceType { get; private set; }    // Type of reference document
    public string? ReferenceNumber { get; private set; }
    public string? BatchNumber { get; private set; }
    public string? SerialNumber { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public string? Notes { get; private set; }
    public StockTransactionStatus Status { get; private set; } = StockTransactionStatus.Pending;

    // Navigation properties
    private readonly StockItem? _stockItem;
    public StockItem? StockItem => _stockItem;

    private readonly Warehouse? _warehouse;
    public Warehouse? Warehouse => _warehouse;

    private readonly Warehouse? _sourceWarehouse;
    public Warehouse? SourceWarehouse => _sourceWarehouse;

    public static StockTransaction Create(
        Guid organizationId,
        string transactionNumber,
        StockTransactionType type,
        DateTime transactionDate,
        Guid stockItemId,
        Guid warehouseId,
        decimal quantity,
        decimal unitCost,
        Guid? sourceWarehouseId = null,
        Guid? referenceId = null,
        string? referenceType = null,
        string? referenceNumber = null,
        string? batchNumber = null,
        string? serialNumber = null,
        DateTime? expiryDate = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(transactionNumber))
            throw new ArgumentException("Transaction number is required", nameof(transactionNumber));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        return new StockTransaction
        {
            OrganizationId = organizationId,
            TransactionNumber = transactionNumber.Trim(),
            Type = type,
            TransactionDate = transactionDate,
            StockItemId = stockItemId,
            WarehouseId = warehouseId,
            SourceWarehouseId = sourceWarehouseId,
            Quantity = quantity,
            UnitCost = unitCost,
            TotalAmount = Math.Round(quantity * unitCost, 2),
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            ReferenceNumber = referenceNumber,
            BatchNumber = batchNumber?.Trim(),
            SerialNumber = serialNumber?.Trim(),
            ExpiryDate = expiryDate,
            Notes = notes?.Trim()
        };
    }

    public void Approve()
    {
        if (Status != StockTransactionStatus.Pending)
            throw new InvalidOperationException("Only pending transactions can be approved");

        Status = StockTransactionStatus.Approved;
        UpdateTimestamp();
    }

    public void Reject(string reason)
    {
        if (Status == StockTransactionStatus.Completed)
            throw new InvalidOperationException("Cannot reject completed transaction");

        Status = StockTransactionStatus.Rejected;
        Notes = string.IsNullOrEmpty(Notes) ? $"Rejected: {reason}" : $"{Notes}\nRejected: {reason}";
        UpdateTimestamp();
    }

    public void Complete()
    {
        if (Status != StockTransactionStatus.Approved)
            throw new InvalidOperationException("Only approved transactions can be completed");

        Status = StockTransactionStatus.Completed;
        UpdateTimestamp();
    }

    public void Cancel()
    {
        if (Status == StockTransactionStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed transaction");

        Status = StockTransactionStatus.Cancelled;
        UpdateTimestamp();
    }
}

public enum StockTransactionStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Completed = 4,
    Cancelled = 5
}
