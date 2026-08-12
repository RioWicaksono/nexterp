using ERP.Domain.Common;
using ERP.Domain.Accounting.Enums;

namespace ERP.Domain.Sales.Entities;

/// <summary>
/// Customer entity
/// </summary>
public class Customer : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string CustomerCode { get; private set; } = string.Empty;
    public string CustomerName { get; private set; } = string.Empty;
    public CustomerType Type { get; private set; } = CustomerType.Individual;
    public string? TaxId { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Mobile { get; private set; }
    public string? Website { get; private set; }
    public string? BillingAddress { get; private set; }
    public string? BillingCity { get; private set; }
    public string? BillingCountry { get; private set; }
    public string? BillingPostalCode { get; private set; }
    public string? ShippingAddress { get; private set; }
    public string? ShippingCity { get; private set; }
    public string? ShippingCountry { get; private set; }
    public string? ShippingPostalCode { get; private set; }
    public Guid? PriceListId { get; private set; }
    public Guid? PaymentTermId { get; private set; }
    public decimal? CreditLimit { get; private set; }
    public decimal OutstandingAmount { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? Notes { get; private set; }

    // Calculated properties
    public decimal AvailableCredit => CreditLimit.HasValue ? CreditLimit.Value - OutstandingAmount : 0;
    public bool IsOverCreditLimit => CreditLimit.HasValue && OutstandingAmount > CreditLimit.Value;

    // Factory method
    public static Customer Create(
        Guid organizationId,
        string customerCode,
        string customerName,
        CustomerType type = CustomerType.Individual,
        string? taxId = null,
        string? email = null,
        string? phone = null)
    {
        if (string.IsNullOrWhiteSpace(customerCode))
            throw new ArgumentException("Customer code is required", nameof(customerCode));

        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name is required", nameof(customerName));

        return new Customer
        {
            OrganizationId = organizationId,
            CustomerCode = customerCode.Trim(),
            CustomerName = customerName.Trim(),
            Type = type,
            TaxId = taxId?.Trim(),
            Email = email?.Trim().ToLowerInvariant(),
            Phone = phone?.Trim()
        };
    }

    public void Update(
        string? customerName = null,
        CustomerType? type = null,
        string? taxId = null,
        string? email = null,
        string? phone = null,
        string? mobile = null,
        string? website = null)
    {
        CustomerName = customerName?.Trim() ?? CustomerName;
        Type = type ?? Type;
        TaxId = taxId?.Trim() ?? TaxId;
        Email = email?.Trim().ToLowerInvariant() ?? Email;
        Phone = phone?.Trim() ?? Phone;
        Mobile = mobile?.Trim() ?? Mobile;
        Website = website?.Trim() ?? Website;
        UpdateTimestamp();
    }

    public void UpdateBillingAddress(string? address, string? city, string? country, string? postalCode)
    {
        BillingAddress = address?.Trim();
        BillingCity = city?.Trim();
        BillingCountry = country?.Trim();
        BillingPostalCode = postalCode?.Trim();
        UpdateTimestamp();
    }

    public void UpdateShippingAddress(string? address, string? city, string? country, string? postalCode)
    {
        ShippingAddress = address?.Trim();
        ShippingCity = city?.Trim();
        ShippingCountry = country?.Trim();
        ShippingPostalCode = postalCode?.Trim();
        UpdateTimestamp();
    }

    public void SetCreditLimit(decimal? limit)
    {
        CreditLimit = limit;
        UpdateTimestamp();
    }

    public void SetPaymentTerm(Guid paymentTermId)
    {
        PaymentTermId = paymentTermId;
        UpdateTimestamp();
    }

    public void AddOutstanding(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));
        OutstandingAmount += amount;
        UpdateTimestamp();
    }

    public void ReduceOutstanding(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));
        OutstandingAmount = Math.Max(0, OutstandingAmount - amount);
        UpdateTimestamp();
    }

    public void Activate() { IsActive = true; UpdateTimestamp(); }
    public void Deactivate() { IsActive = false; UpdateTimestamp(); }
}

public enum CustomerType
{
    Individual = 1,
    Company = 2,
    Government = 3
}

/// <summary>
/// Sales Order header
/// </summary>
public class SalesOrder : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public DateTime OrderDate { get; private set; }
    public DateTime? DeliveryDate { get; private set; }
    public Guid CustomerId { get; private set; }
    public SalesOrderStatus Status { get; private set; } = SalesOrderStatus.Draft;
    public Guid? PriceListId { get; private set; }
    public Guid? PaymentTermId { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? BillingAddress { get; private set; }
    public string? ShippingAddress { get; private set; }
    public string? Notes { get; private set; }
    public Guid? SalesPersonId { get; private set; }
    public Guid? WarehouseId { get; private set; }

    // Navigation properties
    private readonly Customer? _customer;
    public Customer? Customer => _customer;

    private readonly List<SalesOrderLine> _lines = new();
    public IReadOnlyCollection<SalesOrderLine> Lines => _lines.AsReadOnly();

    public static SalesOrder Create(
        Guid organizationId,
        string orderNumber,
        DateTime orderDate,
        Guid customerId,
        DateTime? deliveryDate = null)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("Order number is required", nameof(orderNumber));

        return new SalesOrder
        {
            OrganizationId = organizationId,
            OrderNumber = orderNumber.Trim(),
            OrderDate = orderDate,
            DeliveryDate = deliveryDate,
            CustomerId = customerId
        };
    }

    public void AddLine(SalesOrderLine line)
    {
        if (Status != SalesOrderStatus.Draft)
            throw new InvalidOperationException("Cannot modify submitted order");

        if (!_lines.Any(l => l.StockItemId == line.StockItemId && l.Id == line.Id))
        {
            _lines.Add(line);
            RecalculateTotals();
        }
    }

    public void RemoveLine(Guid lineId)
    {
        if (Status != SalesOrderStatus.Draft)
            throw new InvalidOperationException("Cannot modify submitted order");

        var line = _lines.FirstOrDefault(l => l.Id == lineId);
        if (line != null)
        {
            _lines.Remove(line);
            RecalculateTotals();
        }
    }

    private void RecalculateTotals()
    {
        Subtotal = _lines.Sum(l => l.LineTotal);
        TaxAmount = _lines.Sum(l => l.TaxAmount);
        DiscountAmount = _lines.Sum(l => l.DiscountAmount ?? 0);
        TotalAmount = Subtotal + TaxAmount - DiscountAmount;
        UpdateTimestamp();
    }

    public void Submit()
    {
        if (!_lines.Any())
            throw new InvalidOperationException("Order must have at least one line");

        Status = SalesOrderStatus.Submitted;
        UpdateTimestamp();
    }

    public void Approve()
    {
        if (Status != SalesOrderStatus.Submitted)
            throw new InvalidOperationException("Order must be submitted first");

        Status = SalesOrderStatus.Approved;
        UpdateTimestamp();
    }

    public void Reject(string reason)
    {
        Status = SalesOrderStatus.Rejected;
        Notes = string.IsNullOrEmpty(Notes) ? $"Rejected: {reason}" : $"{Notes}\nRejected: {reason}";
        UpdateTimestamp();
    }

    public void Cancel()
    {
        if (Status == SalesOrderStatus.Delivered || Status == SalesOrderStatus.Invoiced)
            throw new InvalidOperationException("Cannot cancel delivered or invoiced order");

        Status = SalesOrderStatus.Cancelled;
        UpdateTimestamp();
    }

    public void MarkAsDelivered()
    {
        if (Status != SalesOrderStatus.Approved)
            throw new InvalidOperationException("Order must be approved first");

        Status = SalesOrderStatus.Delivered;
        UpdateTimestamp();
    }

    public void MarkAsInvoiced()
    {
        Status = SalesOrderStatus.Invoiced;
        UpdateTimestamp();
    }
}

public enum SalesOrderStatus
{
    Draft = 1,
    Submitted = 2,
    Approved = 3,
    Rejected = 4,
    Cancelled = 5,
    Delivered = 6,
    Invoiced = 7
}

/// <summary>
/// Sales Order Line
/// </summary>
public class SalesOrderLine : BaseEntity
{
    public Guid SalesOrderId { get; private set; }
    public Guid StockItemId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal DeliveredQuantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal? DiscountPercent { get; private set; }
    public decimal? DiscountAmount { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal LineTotal { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }

    // Navigation properties
    private readonly SalesOrder? _salesOrder;
    public SalesOrder? SalesOrder => _salesOrder;

    public static SalesOrderLine Create(
        Guid stockItemId,
        string description,
        decimal quantity,
        decimal unitPrice,
        Guid unitOfMeasureId,
        decimal taxRate = 0,
        decimal? discountPercent = null,
        decimal? discountAmount = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

        var line = new SalesOrderLine
        {
            StockItemId = stockItemId,
            Description = description.Trim(),
            Quantity = quantity,
            UnitPrice = unitPrice,
            DiscountPercent = discountPercent,
            DiscountAmount = discountAmount,
            TaxRate = taxRate,
            UnitOfMeasureId = unitOfMeasureId
        };

        line.CalculateTotals();
        return line;
    }

    public void Update(decimal quantity, decimal unitPrice, decimal taxRate = 0)
    {
        Quantity = quantity;
        UnitPrice = unitPrice;
        TaxRate = taxRate;
        CalculateTotals();
    }

    private void CalculateTotals()
    {
        var grossTotal = Quantity * UnitPrice;
        DiscountAmount = DiscountPercent.HasValue
            ? Math.Round(grossTotal * DiscountPercent.Value / 100, 2)
            : DiscountAmount ?? 0;

        LineTotal = Math.Round(grossTotal - DiscountAmount.Value, 2);
        TaxAmount = Math.Round(LineTotal * TaxRate / 100, 2);
        LineTotal += TaxAmount;
    }

    public void SetDeliveredQuantity(decimal quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(quantity));

        if (quantity > Quantity)
            throw new ArgumentException("Delivered quantity cannot exceed ordered quantity", nameof(quantity));

        DeliveredQuantity = quantity;
    }

    public void SetDelivered()
    {
        DeliveredQuantity = Quantity;
    }
}
