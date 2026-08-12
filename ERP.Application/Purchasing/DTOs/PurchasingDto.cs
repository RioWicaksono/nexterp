using ERP.Application.Common.DTOs;

namespace ERP.Application.Purchasing.DTOs;

/// <summary>
/// Supplier data transfer object
/// </summary>
public class SupplierDto : BaseDto
{
    public Guid OrganizationId { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? BillingAddress { get; set; }
    public string? BillingCity { get; set; }
    public decimal? CreditLimit { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal AvailableCredit { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Purchase Order DTO
/// </summary>
public class PurchaseOrderDto : BaseDto
{
    public Guid OrganizationId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public Guid SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<PurchaseOrderLineDto> Lines { get; set; } = new();
}

/// <summary>
/// Purchase Order Line DTO
/// </summary>
public class PurchaseOrderLineDto
{
    public Guid Id { get; set; }
    public Guid StockItemId { get; set; }
    public string? StockItemName { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
}

/// <summary>
/// DTO for creating a supplier
/// </summary>
public class CreateSupplierDto
{
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string Type { get; set; } = "Company";
    public string? TaxId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? BillingAddress { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingCountry { get; set; }
    public string? BillingPostalCode { get; set; }
    public decimal? CreditLimit { get; set; }
    public Guid? PaymentTermId { get; set; }
}

/// <summary>
/// DTO for creating a purchase order
/// </summary>
public class CreatePurchaseOrderDto
{
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public Guid SupplierId { get; set; }
    public Guid? PaymentTermId { get; set; }
    public string? Notes { get; set; }
    public Guid? WarehouseId { get; set; }
    public List<CreatePurchaseOrderLineDto> Lines { get; set; } = new();
}

/// <summary>
/// DTO for creating a purchase order line
/// </summary>
public class CreatePurchaseOrderLineDto
{
    public Guid StockItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    public decimal? DiscountPercent { get; set; }
}
