using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Sales.Entities;

namespace ERP.Application.Sales.Commands.Customers;

/// <summary>
/// Command to update an existing customer
/// </summary>
public class UpdateCustomerCommand : ICommand<bool>
{
    public Guid Id { get; set; }
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
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Validator for UpdateCustomerCommand
/// </summary>
public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.CustomerCode)
            .NotEmpty().WithMessage("Customer code is required")
            .MaximumLength(50).WithMessage("Customer code cannot exceed 50 characters");

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required")
            .MaximumLength(200).WithMessage("Customer name cannot exceed 200 characters");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Customer type is required")
            .Must(t => new[] { "Individual", "Company", "Government" }.Contains(t, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid customer type. Valid values: Individual, Company, Government");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Invalid email format");

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).When(x => x.CreditLimit.HasValue)
            .WithMessage("Credit limit cannot be negative");
    }
}

/// <summary>
/// Handler for UpdateCustomerCommand
/// </summary>
public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateCustomerCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUser.OrganizationId
            ?? throw new UnauthorizedAccessException("User is not associated with an organization");

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted && c.OrganizationId == organizationId, cancellationToken);

        if (customer == null)
            return Result<bool>.Failure("Customer not found");

        // Check for duplicate code
        var existingCode = await _context.Customers
            .AnyAsync(c => c.OrganizationId == organizationId &&
                          c.Id != request.Id &&
                          c.CustomerCode == request.CustomerCode.ToUpperInvariant() &&
                          !c.IsDeleted, cancellationToken);

        if (existingCode)
            return Result<bool>.Failure("Customer code already exists");

        // Parse customer type
        if (!Enum.TryParse<CustomerType>(request.Type, true, out var customerType))
            customerType = CustomerType.Individual;

        // Update customer using the entity's Update method
        customer.Update(
            customerName: request.CustomerName,
            type: customerType,
            taxId: request.TaxId,
            email: request.Email,
            phone: request.Phone,
            mobile: request.Mobile);

        // Update billing address
        if (!string.IsNullOrWhiteSpace(request.BillingAddress))
            customer.UpdateBillingAddress(
                request.BillingAddress,
                request.BillingCity,
                request.BillingCountry,
                request.BillingPostalCode);

        // Update credit limit
        if (request.CreditLimit.HasValue)
            customer.SetCreditLimit(request.CreditLimit.Value);

        // Update payment term
        if (request.PaymentTermId.HasValue)
            customer.SetPaymentTerm(request.PaymentTermId.Value);

        // Update status
        if (request.IsActive)
            customer.Activate();
        else
            customer.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
