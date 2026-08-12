using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Application.Sales.DTOs;
using ERP.Domain.Sales.Entities;

namespace ERP.Application.Sales.Commands.SalesOrders;

/// <summary>
/// Command to create a new sales order
/// </summary>
public class CreateSalesOrderCommand : ICommand<Guid>
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
/// Validator for CreateSalesOrderCommand
/// </summary>
public class CreateSalesOrderCommandValidator : AbstractValidator<CreateSalesOrderCommand>
{
    public CreateSalesOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer is required");

        RuleFor(x => x.OrderDate)
            .NotEmpty().WithMessage("Order date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("Order date cannot be in the future");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("Sales order must have at least one line")
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
/// Handler for CreateSalesOrderCommand
/// </summary>
public class CreateSalesOrderCommandHandler : IRequestHandler<CreateSalesOrderCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateSalesOrderCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUser.OrganizationId
            ?? throw new UnauthorizedAccessException("User is not associated with an organization");

        // Check if customer exists and is active
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId && !c.IsDeleted && c.OrganizationId == organizationId, cancellationToken);

        if (customer == null)
            return Result<Guid>.Failure("Customer not found");

        if (!customer.IsActive)
            return Result<Guid>.Failure("Customer is inactive");

        // Check credit limit
        if (customer.IsOverCreditLimit)
            return Result<Guid>.Failure("Customer has exceeded credit limit");

        // Validate stock items
        var stockItemIds = request.Lines.Select(l => l.StockItemId).Distinct().ToList();
        var stockItems = await _context.StockItems
            .Where(s => stockItemIds.Contains(s.Id) && !s.IsDeleted && s.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        if (stockItems.Count != stockItemIds.Count)
            return Result<Guid>.Failure("One or more stock items are invalid");

        // Generate order number
        var orderNumber = await GenerateOrderNumberAsync(organizationId, cancellationToken);

        // Create sales order
        var order = SalesOrder.Create(
            organizationId,
            orderNumber,
            request.OrderDate,
            request.CustomerId,
            request.DeliveryDate);

        // Add lines
        foreach (var lineDto in request.Lines)
        {
            var line = SalesOrderLine.Create(
                lineDto.StockItemId,
                lineDto.Description,
                lineDto.Quantity,
                lineDto.UnitPrice,
                lineDto.UnitOfMeasureId,
                lineDto.TaxRate,
                lineDto.DiscountPercent);

            order.AddLine(line);
        }

        _context.SalesOrders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(order.Id);
    }

    private async Task<string> GenerateOrderNumberAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month.ToString("D2");

        var lastOrder = await _context.SalesOrders
            .Where(o => o.OrganizationId == organizationId)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var lastNumber = 0;
        if (lastOrder != null)
        {
            var parts = lastOrder.OrderNumber.Split('-');
            if (parts.Length >= 3 && parts[0] == "SO")
            {
                int.TryParse(parts[2], out lastNumber);
            }
        }

        return $"SO{year}{month}-{(lastNumber + 1):D5}";
    }
}
