using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Application.Purchasing.DTOs;
using ERP.Domain.Purchasing.Entities;

namespace ERP.Application.Purchasing.Commands.PurchaseOrders;

/// <summary>
/// Command to create a new purchase order
/// </summary>
public class CreatePurchaseOrderCommand : ICommand<Guid>
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
/// Validator for CreatePurchaseOrderCommand
/// </summary>
public class CreatePurchaseOrderCommandValidator : AbstractValidator<CreatePurchaseOrderCommand>
{
    public CreatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage("Supplier is required");

        RuleFor(x => x.OrderDate)
            .NotEmpty().WithMessage("Order date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("Order date cannot be in the future");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("Purchase order must have at least one line")
            .Must(lines => lines.All(l => l.Quantity > 0))
            .WithMessage("All line quantities must be greater than zero");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.StockItemId)
                .NotEmpty().WithMessage("Stock item is required for each line");

            line.RuleFor(l => l.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative");

            line.RuleFor(l => l.TaxRate)
                .GreaterThanOrEqualTo(0).WithMessage("Tax rate cannot be negative")
                .LessThanOrEqualTo(100).WithMessage("Tax rate cannot exceed 100%");
        });
    }
}

/// <summary>
/// Handler for CreatePurchaseOrderCommand
/// </summary>
public class CreatePurchaseOrderCommandHandler : IRequestHandler<CreatePurchaseOrderCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreatePurchaseOrderCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        // Return failure if user is not associated with an organization
        if (_currentUser.OrganizationId == null)
            return Result<Guid>.Failure("User is not associated with an organization");

        var organizationId = _currentUser.OrganizationId.Value;

        // Check if supplier exists and is active
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == request.SupplierId && !s.IsDeleted && s.OrganizationId == organizationId, cancellationToken);

        if (supplier == null)
            return Result<Guid>.Failure("Supplier not found");

        if (!supplier.IsActive)
            return Result<Guid>.Failure("Supplier is inactive");

        // Validate stock items
        var stockItemIds = request.Lines.Select(l => l.StockItemId).Distinct().ToList();
        var stockItems = await _context.StockItems
            .Where(s => stockItemIds.Contains(s.Id) && !s.IsDeleted && s.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        if (stockItems.Count != stockItemIds.Count)
            return Result<Guid>.Failure("One or more stock items are invalid");

        // Generate order number
        var orderNumber = await GenerateOrderNumberAsync(organizationId, cancellationToken);

        // Create purchase order
        var order = PurchaseOrder.Create(
            organizationId,
            orderNumber,
            request.OrderDate,
            request.SupplierId,
            request.ExpectedDeliveryDate);

        // Add lines
        foreach (var lineDto in request.Lines)
        {
            var line = PurchaseOrderLine.Create(
                lineDto.StockItemId,
                lineDto.Description,
                lineDto.Quantity,
                lineDto.UnitPrice,
                lineDto.UnitOfMeasureId,
                lineDto.TaxRate,
                lineDto.DiscountPercent);

            order.AddLine(line);
        }

        _context.PurchaseOrders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(order.Id);
    }

    private async Task<string> GenerateOrderNumberAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month.ToString("D2");

        var lastOrder = await _context.PurchaseOrders
            .Where(o => o.OrganizationId == organizationId)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var lastNumber = 0;
        if (lastOrder != null)
        {
            var parts = lastOrder.OrderNumber.Split('-');
            if (parts.Length >= 3 && parts[0] == "PO")
            {
                int.TryParse(parts[2], out lastNumber);
            }
        }

        return $"PO{year}{month}-{(lastNumber + 1):D5}";
    }
}
