namespace ERP.Domain.Inventory.Enums;

/// <summary>
/// Types of stock transactions in inventory management
/// </summary>
public enum StockTransactionType
{
    PurchaseReceipt = 1,    // Stock IN from purchase
    PurchaseReturn = 2,      // Stock OUT from return to supplier
    SalesDelivery = 3,       // Stock OUT from sales
    SalesReturn = 4,          // Stock IN from customer return
    StockTransfer = 5,        // Stock movement between warehouses
    StockAdjustment = 6,      // Manual adjustment (IN/OUT)
    StockOpening = 7,         // Initial stock entry
    StockDamage = 8,          // Stock OUT due to damage
    StockExpired = 9          // Stock OUT due to expiration
}

/// <summary>
/// Valuation methods for inventory
/// </summary>
public enum ValuationMethod
{
    AverageCost = 1,
    FIFO = 2,           // First In, First Out
    LIFO = 3,           // Last In, First Out (not allowed in some countries)
    StandardCost = 4
}

/// <summary>
/// Unit of Measure types
/// </summary>
public enum UomType
{
    Quantity = 1,       // Countable items (pcs, boxes)
    Weight = 2,         // Weight (kg, lb)
    Volume = 3,         // Volume (L, ml, gallon)
    Length = 4,         // Length (m, cm, inch)
    Area = 5,           // Area (sqm, sqft)
    Time = 6,           // Time (hours, days)
    Currency = 7        // Currency (for price per unit)
}
