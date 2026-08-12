using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Purchasing.Entities;

namespace ERP.Application.Purchasing.Commands.Suppliers;

/// <summary>
/// Command to update an existing supplier
/// </summary>
public class UpdateSupplierCommand : ICommand<bool>
{
    public Guid Id { get; set; }
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
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Validator for UpdateSupplierCommand
/// </summary>
public class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Supplier ID is required");

        RuleFor(x => x.SupplierCode)
            .NotEmpty().WithMessage("Supplier code is required")
            .MaximumLength(50).WithMessage("Supplier code cannot exceed 50 characters");

        RuleFor(x => x.SupplierName)
            .NotEmpty().WithMessage("Supplier name is required")
            .MaximumLength(200).WithMessage("Supplier name cannot exceed 200 characters");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Supplier type is required")
            .Must(t => new[] { "Individual", "Company", "Government" }.Contains(t, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid supplier type. Valid values: Individual, Company, Government");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Invalid email format");

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).When(x => x.CreditLimit.HasValue)
            .WithMessage("Credit limit cannot be negative");
    }
}

/// <summary>
/// Handler for UpdateSupplierCommand
/// </summary>
public class UpdateSupplierCommandHandler : IRequestHandler<UpdateSupplierCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateSupplierCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUser.OrganizationId
            ?? throw new UnauthorizedAccessException("User is not associated with an organization");

        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == request.Id && !s.IsDeleted && s.OrganizationId == organizationId, cancellationToken);

        if (supplier == null)
            return Result<bool>.Failure("Supplier not found");

        var exists = await _context.Suppliers
            .AnyAsync(s => s.OrganizationId == organizationId &&
                          s.Id != request.Id &&
                          (s.SupplierCode == request.SupplierCode || s.SupplierName == request.SupplierName) &&
                          !s.IsDeleted, cancellationToken);

        if (exists)
            return Result<bool>.Failure("Another supplier with this code or name already exists");

        // Parse supplier type
        if (!Enum.TryParse<SupplierType>(request.Type, true, out var supplierType))
            supplierType = SupplierType.Company;

        // Update using the entity's Update method
        supplier.Update(
            supplierName: request.SupplierName,
            type: supplierType,
            taxId: request.TaxId,
            email: request.Email,
            phone: request.Phone,
            mobile: request.Mobile);

        // Update billing address
        if (!string.IsNullOrWhiteSpace(request.BillingAddress))
            supplier.UpdateBillingAddress(request.BillingAddress, request.BillingCity, request.BillingCountry, request.BillingPostalCode);

        // Update credit limit
        if (request.CreditLimit.HasValue)
            supplier.SetCreditLimit(request.CreditLimit.Value);

        // Update status
        if (request.IsActive)
            supplier.Activate();
        else
            supplier.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
