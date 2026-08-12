using ERP.Application.Common.DTOs;

namespace ERP.Application.Sales.DTOs;

/// <summary>
/// Customer data transfer object
/// </summary>
public class CustomerDto : BaseDto
{
    public Guid OrganizationId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? BillingAddress { get; set; }
    public string? BillingCity { get; set; }
    public string? CreditLimit { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal AvailableCredit { get; set; }
    public bool IsOverCreditLimit { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Sales Order data transfer object
/// </summary>
public class SalesOrderDto : BaseDto
{
    public Guid OrganizationId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<SalesOrderLineDto> Lines { get; set; } = new();
}

/// <summary>
/// Sales Order Line DTO
/// </summary>
public class SalesOrderLineDto
{
    public Guid Id { get; set; }
    public Guid StockItemId { get; set; }
    public string? StockItemName { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal DeliveredQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
}

/// <summary>
/// Sales Invoice DTO
/// </summary>
public class SalesInvoiceDto : BaseDto
{
    public Guid OrganizationId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public bool IsPaid { get; set; }
    public bool IsOverdue { get; set; }
}

/// <summary>
/// DTO for creating a customer
/// </summary>
public class CreateCustomerDto
{
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Type { get; set; } = "Individual";
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
/// DTO for creating a sales order
/// </summary>
public class CreateSalesOrderDto
{
    public DateTime OrderDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? PriceListId { get; set; }
    public Guid? PaymentTermId { get; set; }
    public string? Notes { get; set; }
    public Guid? WarehouseId { get; set; }
    public List<CreateSalesOrderLineDto> Lines { get; set; } = new();
}

/// <summary>
/// DTO for creating a sales order line
/// </summary>
public class CreateSalesOrderLineDto
{
    public Guid StockItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    public decimal? DiscountPercent { get; set; }
}
