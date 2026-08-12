using ERP.Domain.Common;

namespace ERP.Domain.Sales.Entities;

/// <summary>
/// Sales Invoice header
/// </summary>
public class SalesInvoice : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string InvoiceNumber { get; private set; } = string.Empty;
    public DateTime InvoiceDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid? SalesOrderId { get; private set; }
    public SalesInvoiceStatus Status { get; private set; } = SalesInvoiceStatus.Draft;
    public InvoiceType Type { get; private set; } = InvoiceType.Invoice;
    public Guid? PriceListId { get; private set; }
    public Guid? PaymentTermId { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public decimal OutstandingAmount { get; private set; }
    public string? BillingAddress { get; private set; }
    public string? Notes { get; private set; }
    public string? Terms { get; private set; }
    public Guid? SalesPersonId { get; private set; }
    public DateTime? PrintedAt { get; private set; }
    public DateTime? SentAt { get; private set; }

    // Navigation properties
    private readonly Customer? _customer;
    public Customer? Customer => _customer;

    private readonly List<SalesInvoiceLine> _lines = new();
    public IReadOnlyCollection<SalesInvoiceLine> Lines => _lines.AsReadOnly();

    private readonly List<PaymentDetail> _payments = new();
    public IReadOnlyCollection<PaymentDetail> Payments => _payments.AsReadOnly();

    public bool IsPaid => OutstandingAmount <= 0;
    public bool IsOverdue => !IsPaid && DueDate < DateTime.UtcNow;

    public static SalesInvoice Create(
        Guid organizationId,
        string invoiceNumber,
        DateTime invoiceDate,
        DateTime dueDate,
        Guid customerId,
        InvoiceType type = InvoiceType.Invoice,
        Guid? salesOrderId = null)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new ArgumentException("Invoice number is required", nameof(invoiceNumber));

        return new SalesInvoice
        {
            OrganizationId = organizationId,
            InvoiceNumber = invoiceNumber.Trim(),
            InvoiceDate = invoiceDate,
            DueDate = dueDate,
            CustomerId = customerId,
            Type = type,
            SalesOrderId = salesOrderId
        };
    }

    public void AddLine(SalesInvoiceLine line)
    {
        if (Status != SalesInvoiceStatus.Draft)
            throw new InvalidOperationException("Cannot modify submitted invoice");

        if (!_lines.Any(l => l.StockItemId == line.StockItemId && l.Id == line.Id))
        {
            _lines.Add(line);
            RecalculateTotals();
        }
    }

    public void RemoveLine(Guid lineId)
    {
        if (Status != SalesInvoiceStatus.Draft)
            throw new InvalidOperationException("Cannot modify submitted invoice");

        var line = _lines.FirstOrDefault(l => l.Id == lineId);
        if (line != null)
        {
            _lines.Remove(line);
            RecalculateTotals();
        }
    }

    private void RecalculateTotals()
    {
        Subtotal = _lines.Sum(l => l.Subtotal);
        TaxAmount = _lines.Sum(l => l.TaxAmount);
        DiscountAmount = _lines.Sum(l => l.DiscountAmount ?? 0);
        TotalAmount = Subtotal + TaxAmount - DiscountAmount;
        OutstandingAmount = TotalAmount - PaidAmount;
        UpdateTimestamp();
    }

    public void Submit()
    {
        if (!_lines.Any())
            throw new InvalidOperationException("Invoice must have at least one line");

        Status = SalesInvoiceStatus.Submitted;
        UpdateTimestamp();
    }

    public void Post()
    {
        if (Status != SalesInvoiceStatus.Submitted)
            throw new InvalidOperationException("Invoice must be submitted first");

        Status = SalesInvoiceStatus.Posted;
        SentAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void Cancel()
    {
        if (Status == SalesInvoiceStatus.Paid || Status == SalesInvoiceStatus.Cancelled)
            throw new InvalidOperationException("Cannot cancel paid or already cancelled invoice");

        Status = SalesInvoiceStatus.Cancelled;
        UpdateTimestamp();
    }

    public void RecordPayment(decimal amount, string? reference = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Payment amount must be positive", nameof(amount));

        if (amount > OutstandingAmount)
            throw new ArgumentException("Payment amount cannot exceed outstanding amount");

        PaidAmount += amount;
        OutstandingAmount = TotalAmount - PaidAmount;

        _payments.Add(PaymentDetail.Create(Id, amount, reference));

        if (OutstandingAmount <= 0)
        {
            Status = SalesInvoiceStatus.Paid;
        }

        UpdateTimestamp();
    }

    public void RecordPayment(PaymentDetail payment)
    {
        RecordPayment(payment.Amount, payment.Reference);
    }

    public void MarkAsPrinted()
    {
        PrintedAt = DateTime.UtcNow;
    }
}

public enum SalesInvoiceStatus
{
    Draft = 1,
    Submitted = 2,
    Posted = 3,
    Cancelled = 4,
    Paid = 5
}

public enum InvoiceType
{
    Invoice = 1,
    CreditNote = 2,   // Sales return
    DebitNote = 3     // Adjustments
}

/// <summary>
/// Sales Invoice Line
/// </summary>
public class SalesInvoiceLine : BaseEntity
{
    public Guid SalesInvoiceId { get; private set; }
    public Guid StockItemId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal? DiscountPercent { get; private set; }
    public decimal? DiscountAmount { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal LineTotal { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }

    // Navigation properties
    private readonly SalesInvoice? _salesInvoice;
    public SalesInvoice? SalesInvoice => _salesInvoice;

    public static SalesInvoiceLine Create(
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

        var line = new SalesInvoiceLine
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
        Subtotal = Math.Round(Quantity * UnitPrice, 2);

        if (DiscountPercent.HasValue)
        {
            DiscountAmount = Math.Round(Subtotal * DiscountPercent.Value / 100, 2);
            Subtotal -= DiscountAmount.Value;
        }

        TaxAmount = Math.Round(Subtotal * TaxRate / 100, 2);
        LineTotal = Math.Round(Subtotal + TaxAmount, 2);
    }
}

/// <summary>
/// Payment detail for invoice
/// </summary>
public class PaymentDetail : BaseEntity
{
    public Guid SalesInvoiceId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime PaymentDate { get; private set; }
    public string? Reference { get; private set; }
    public string? Notes { get; private set; }

    // Navigation properties
    private readonly SalesInvoice? _salesInvoice;
    public SalesInvoice? SalesInvoice => _salesInvoice;

    public static PaymentDetail Create(Guid salesInvoiceId, decimal amount, string? reference = null) => new()
    {
        SalesInvoiceId = salesInvoiceId,
        Amount = Math.Round(amount, 2),
        PaymentDate = DateTime.UtcNow,
        Reference = reference?.Trim()
    };
}
