using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Application.Inventory.DTOs;
using ERP.Domain.Inventory.Entities;

namespace ERP.Application.Inventory.Commands.Warehouses;

/// <summary>
/// Command to create a new warehouse
/// </summary>
public class CreateWarehouseCommand : ICommand<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsDefault { get; set; }
    public bool AllowsNegativeStock { get; set; }
}

/// <summary>
/// Validator for CreateWarehouseCommand
/// </summary>
public class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Warehouse name is required")
            .MaximumLength(200).WithMessage("Warehouse name cannot exceed 200 characters");

        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Code cannot exceed 50 characters");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Invalid email format");

        RuleFor(x => x.Phone)
            .Matches(@"^\+?[0-9]{10,15}$").When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Invalid phone number format");
    }
}

/// <summary>
/// Handler for CreateWarehouseCommand
/// </summary>
public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateWarehouseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUser.OrganizationId
            ?? throw new UnauthorizedAccessException("User is not associated with an organization");

        // Check if code already exists
        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var existingCode = await _context.Warehouses
                .AnyAsync(w => w.OrganizationId == organizationId &&
                              w.Code == request.Code.ToUpperInvariant() &&
                              !w.IsDeleted, cancellationToken);

            if (existingCode)
                return Result<Guid>.Failure("Warehouse code already exists");
        }

        // Check if this is the first warehouse (make it default)
        var existingWarehouses = await _context.Warehouses
            .AnyAsync(w => w.OrganizationId == organizationId && !w.IsDeleted, cancellationToken);

        var isFirstWarehouse = !existingWarehouses;

        // Create warehouse
        var warehouse = Warehouse.Create(
            organizationId,
            request.Name,
            request.Code,
            request.Description,
            request.Address,
            request.City,
            request.Country,
            request.Phone,
            request.Email,
            request.IsDefault || isFirstWarehouse);

        if (request.AllowsNegativeStock)
            warehouse.AllowNegativeStock();
        else
            warehouse.DisallowNegativeStock();

        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(warehouse.Id);
    }
}
