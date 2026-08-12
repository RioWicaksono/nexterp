using ERP.Domain.Common;

namespace ERP.Domain.Purchasing.Entities;

/// <summary>
/// Supplier entity
/// </summary>
public class Supplier : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string SupplierCode { get; private set; } = string.Empty;
    public string SupplierName { get; private set; } = string.Empty;
    public SupplierType Type { get; private set; } = SupplierType.Company;
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
    public Guid? PaymentTermId { get; private set; }
    public decimal? CreditLimit { get; private set; }
    public decimal OutstandingAmount { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? Notes { get; private set; }
    public string? BankName { get; private set; }
    public string? BankAccountNumber { get; private set; }
    public string? BankAccountName { get; private set; }

    // Calculated properties
    public decimal AvailableCredit => CreditLimit.HasValue ? CreditLimit.Value - OutstandingAmount : 0;

    public static Supplier Create(
        Guid organizationId,
        string supplierCode,
        string supplierName,
        SupplierType type = SupplierType.Company,
        string? taxId = null,
        string? email = null,
        string? phone = null)
    {
        if (string.IsNullOrWhiteSpace(supplierCode))
            throw new ArgumentException("Supplier code is required", nameof(supplierCode));

        if (string.IsNullOrWhiteSpace(supplierName))
            throw new ArgumentException("Supplier name is required", nameof(supplierName));

        return new Supplier
        {
            OrganizationId = organizationId,
            SupplierCode = supplierCode.Trim(),
            SupplierName = supplierName.Trim(),
            Type = type,
            TaxId = taxId?.Trim(),
            Email = email?.Trim().ToLowerInvariant(),
            Phone = phone?.Trim()
        };
    }

    public void Update(
        string? supplierName = null,
        SupplierType? type = null,
        string? taxId = null,
        string? email = null,
        string? phone = null,
        string? mobile = null,
        string? website = null)
    {
        SupplierName = supplierName?.Trim() ?? SupplierName;
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

    public void SetPaymentTerm(Guid paymentTermId)
    {
        PaymentTermId = paymentTermId;
        UpdateTimestamp();
    }

    public void SetCreditLimit(decimal? limit)
    {
        CreditLimit = limit;
        UpdateTimestamp();
    }

    public void SetBankDetails(string? bankName, string? accountNumber, string? accountName)
    {
        BankName = bankName?.Trim();
        BankAccountNumber = accountNumber?.Trim();
        BankAccountName = accountName?.Trim();
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

public enum SupplierType
{
    Individual = 1,
    Company = 2,
    Government = 3
}

/// <summary>
/// Purchase Order header
/// </summary>
public class PurchaseOrder : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public DateTime OrderDate { get; private set; }
    public DateTime? ExpectedDeliveryDate { get; private set; }
    public Guid SupplierId { get; private set; }
    public PurchaseOrderStatus Status { get; private set; } = PurchaseOrderStatus.Draft;
    public Guid? PaymentTermId { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? BillingAddress { get; private set; }
    public string? ShippingAddress { get; private set; }
    public string? Notes { get; private set; }
    public Guid? WarehouseId { get; private set; }

    // Navigation properties
    private readonly Supplier? _supplier;
    public Supplier? Supplier => _supplier;

    private readonly List<PurchaseOrderLine> _lines = new();
    public IReadOnlyCollection<PurchaseOrderLine> Lines => _lines.AsReadOnly();

    public static PurchaseOrder Create(
        Guid organizationId,
        string orderNumber,
        DateTime orderDate,
        Guid supplierId,
        DateTime? expectedDeliveryDate = null)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("Order number is required", nameof(orderNumber));

        return new PurchaseOrder
        {
            OrganizationId = organizationId,
            OrderNumber = orderNumber.Trim(),
            OrderDate = orderDate,
            ExpectedDeliveryDate = expectedDeliveryDate,
            SupplierId = supplierId
        };
    }

    public void AddLine(PurchaseOrderLine line)
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new InvalidOperationException("Cannot modify submitted order");

        if (!_lines.Any(l => l.StockItemId == line.StockItemId && l.Id == line.Id))
        {
            _lines.Add(line);
            RecalculateTotals();
        }
    }

    public void RemoveLine(Guid lineId)
    {
        if (Status != PurchaseOrderStatus.Draft)
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

        Status = PurchaseOrderStatus.Submitted;
        UpdateTimestamp();
    }

    public void Approve()
    {
        if (Status != PurchaseOrderStatus.Submitted)
            throw new InvalidOperationException("Order must be submitted first");

        Status = PurchaseOrderStatus.Approved;
        UpdateTimestamp();
    }

    public void Reject(string reason)
    {
        Status = PurchaseOrderStatus.Rejected;
        Notes = string.IsNullOrEmpty(Notes) ? $"Rejected: {reason}" : $"{Notes}\nRejected: {reason}";
        UpdateTimestamp();
    }

    public void Cancel()
    {
        if (Status == PurchaseOrderStatus.Received || Status == PurchaseOrderStatus.Invoiced)
            throw new InvalidOperationException("Cannot cancel received or invoiced order");

        Status = PurchaseOrderStatus.Cancelled;
        UpdateTimestamp();
    }

    public void MarkAsReceived()
    {
        if (Status != PurchaseOrderStatus.Approved)
            throw new InvalidOperationException("Order must be approved first");

        Status = PurchaseOrderStatus.Received;
        UpdateTimestamp();
    }

    public void MarkAsInvoiced()
    {
        Status = PurchaseOrderStatus.Invoiced;
        UpdateTimestamp();
    }
}

public enum PurchaseOrderStatus
{
    Draft = 1,
    Submitted = 2,
    Approved = 3,
    Rejected = 4,
    Cancelled = 5,
    Received = 6,
    Invoiced = 7
}

/// <summary>
/// Purchase Order Line
/// </summary>
public class PurchaseOrderLine : BaseEntity
{
    public Guid PurchaseOrderId { get; private set; }
    public Guid StockItemId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal ReceivedQuantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal? DiscountPercent { get; private set; }
    public decimal? DiscountAmount { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal LineTotal { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }

    // Navigation properties
    private readonly PurchaseOrder? _purchaseOrder;
    public PurchaseOrder? PurchaseOrder => _purchaseOrder;

    public static PurchaseOrderLine Create(
        Guid stockItemId,
        string description,
        decimal quantity,
        decimal unitPrice,
        Guid unitOfMeasureId,
        decimal taxRate = 0,
        decimal? discountPercent = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        var line = new PurchaseOrderLine
        {
            StockItemId = stockItemId,
            Description = description.Trim(),
            Quantity = quantity,
            UnitPrice = unitPrice,
            DiscountPercent = discountPercent,
            TaxRate = taxRate,
            UnitOfMeasureId = unitOfMeasureId
        };

        line.CalculateTotals();
        return line;
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

    public void Update(decimal quantity, decimal unitPrice, decimal taxRate = 0)
    {
        Quantity = quantity;
        UnitPrice = unitPrice;
        TaxRate = taxRate;
        CalculateTotals();
    }

    public void SetReceivedQuantity(decimal quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(quantity));

        if (quantity > Quantity)
            throw new ArgumentException("Received quantity cannot exceed ordered quantity", nameof(quantity));

        ReceivedQuantity = quantity;
    }
}
