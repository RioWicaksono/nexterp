using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Application.Inventory.DTOs;
using ERP.Domain.Inventory.Entities;
using ERP.Domain.Inventory.Enums;

namespace ERP.Application.Inventory.Commands.StockItems;

/// <summary>
/// Command to create a new stock item
/// </summary>
public class CreateStockItemCommand : ICommand<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Barcode { get; set; }
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal? MinimumStock { get; set; }
    public decimal? MaximumStock { get; set; }
    public decimal? StandardCost { get; set; }
    public decimal? StandardPrice { get; set; }
    public string ValuationMethod { get; set; } = "AverageCost";
    public bool TrackSerials { get; set; }
    public bool TrackBatch { get; set; }
}

/// <summary>
/// Validator for CreateStockItemCommand
/// </summary>
public class CreateStockItemCommandValidator : AbstractValidator<CreateStockItemCommand>
{
    public CreateStockItemCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Item name is required")
            .MaximumLength(200).WithMessage("Item name cannot exceed 200 characters");

        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Code cannot exceed 50 characters");

        RuleFor(x => x.Barcode)
            .MaximumLength(100).WithMessage("Barcode cannot exceed 100 characters");

        RuleFor(x => x.UnitOfMeasureId)
            .NotEmpty().WithMessage("Unit of measure is required");

        RuleFor(x => x.StandardCost)
            .GreaterThanOrEqualTo(0).WithMessage("Standard cost cannot be negative");

        RuleFor(x => x.StandardPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Standard price cannot be negative");

        RuleFor(x => x.MinimumStock)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum stock cannot be negative");

        RuleFor(x => x.MaximumStock)
            .GreaterThanOrEqualTo(0).WithMessage("Maximum stock cannot be negative")
            .GreaterThan(x => x.MinimumStock)
            .When(x => x.MinimumStock.HasValue && x.MaximumStock.HasValue)
            .WithMessage("Maximum stock must be greater than minimum stock");

        RuleFor(x => x.ValuationMethod)
            .Must(vm => Enum.TryParse<ValuationMethod>(vm, true, out _))
            .WithMessage("Invalid valuation method");
    }
}

/// <summary>
/// Handler for CreateStockItemCommand
/// </summary>
public class CreateStockItemCommandHandler : IRequestHandler<CreateStockItemCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateStockItemCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateStockItemCommand request, CancellationToken cancellationToken)
    {
        // Return failure if user is not associated with an organization
        if (_currentUser.OrganizationId == null)
            return Result<Guid>.Failure("User is not associated with an organization");

        var organizationId = _currentUser.OrganizationId.Value;

        // Check if code already exists
        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var existingCode = await _context.StockItems
                .AnyAsync(s => s.OrganizationId == organizationId &&
                              s.Code == request.Code.ToUpperInvariant() &&
                              !s.IsDeleted, cancellationToken);

            if (existingCode)
                return Result<Guid>.Failure("Stock item code already exists");
        }

        // Check if barcode already exists
        if (!string.IsNullOrWhiteSpace(request.Barcode))
        {
            var existingBarcode = await _context.StockItems
                .AnyAsync(s => s.OrganizationId == organizationId &&
                              s.Barcode == request.Barcode &&
                              !s.IsDeleted, cancellationToken);

            if (existingBarcode)
                return Result<Guid>.Failure("Barcode already exists");
        }

        // Check if UOM exists
        var uomExists = await _context.UnitOfMeasures
            .AnyAsync(u => u.Id == request.UnitOfMeasureId && !u.IsDeleted, cancellationToken);

        if (!uomExists)
            return Result<Guid>.Failure("Unit of measure not found");

        // Parse valuation method
        if (!Enum.TryParse<ValuationMethod>(request.ValuationMethod, true, out var valuationMethod))
            return Result<Guid>.Failure("Invalid valuation method");

        // Create stock item
        var stockItem = StockItem.Create(
            organizationId,
            request.Name,
            request.UnitOfMeasureId,
            request.Code,
            request.Barcode,
            request.Description,
            request.CategoryId,
            valuationMethod,
            request.ReorderLevel,
            request.MinimumStock,
            request.MaximumStock,
            request.StandardCost,
            request.StandardPrice);

        if (request.TrackSerials)
            stockItem.EnableSerialTracking();

        if (request.TrackBatch)
            stockItem.EnableBatchTracking();

        _context.StockItems.Add(stockItem);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(stockItem.Id);
    }
}
