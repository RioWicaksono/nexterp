using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Purchasing.Entities;

namespace ERP.Application.Purchasing.Commands.Suppliers;

/// <summary>
/// Command to create a new supplier
/// </summary>
public class CreateSupplierCommand : ICommand<Guid>
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
/// Validator for CreateSupplierCommand
/// </summary>
public class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
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
/// Handler for CreateSupplierCommand
/// </summary>
public class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateSupplierCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUser.OrganizationId
            ?? throw new UnauthorizedAccessException("User is not associated with an organization");

        var exists = await _context.Suppliers
            .AnyAsync(s => s.OrganizationId == organizationId &&
                          (s.SupplierCode == request.SupplierCode || s.SupplierName == request.SupplierName) &&
                          !s.IsDeleted, cancellationToken);

        if (exists)
            return Result<Guid>.Failure("Supplier with this code or name already exists");

        // Parse supplier type
        if (!Enum.TryParse<SupplierType>(request.Type, true, out var supplierType))
            supplierType = SupplierType.Company;

        var supplier = Supplier.Create(
            organizationId,
            request.SupplierCode,
            request.SupplierName,
            supplierType,
            request.TaxId,
            request.Email,
            request.Phone);

        if (!string.IsNullOrEmpty(request.Mobile))
            supplier.Update(mobile: request.Mobile);

        if (!string.IsNullOrEmpty(request.BillingAddress))
            supplier.UpdateBillingAddress(request.BillingAddress, request.BillingCity, request.BillingCountry, request.BillingPostalCode);

        if (request.CreditLimit.HasValue)
            supplier.SetCreditLimit(request.CreditLimit.Value);

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(supplier.Id);
    }
}
