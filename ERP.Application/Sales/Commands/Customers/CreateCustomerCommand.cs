using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Application.Sales.DTOs;
using ERP.Domain.Sales.Entities;

namespace ERP.Application.Sales.Commands.Customers;

/// <summary>
/// Command to create a new customer
/// </summary>
public class CreateCustomerCommand : ICommand<Guid>
{
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
}

/// <summary>
/// Validator for CreateCustomerCommand
/// </summary>
public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
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

        RuleFor(x => x.Phone)
            .Matches(@"^\+?[0-9]{10,15}$").When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Invalid phone number format");

        RuleFor(x => x.Mobile)
            .Matches(@"^\+?[0-9]{10,15}$").When(x => !string.IsNullOrEmpty(x.Mobile))
            .WithMessage("Invalid mobile number format");

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).When(x => x.CreditLimit.HasValue)
            .WithMessage("Credit limit cannot be negative");

        RuleFor(x => x.TaxId)
            .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.TaxId))
            .WithMessage("Tax ID cannot exceed 50 characters");
    }
}

/// <summary>
/// Handler for CreateCustomerCommand
/// </summary>
public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateCustomerCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        // Return failure if user is not associated with an organization
        if (_currentUser.OrganizationId == null)
            return Result<Guid>.Failure("User is not associated with an organization");

        var organizationId = _currentUser.OrganizationId.Value;

        // Check if customer code already exists
        var existingCode = await _context.Customers
            .AnyAsync(c => c.OrganizationId == organizationId &&
                          c.CustomerCode == request.CustomerCode.ToUpperInvariant() &&
                          !c.IsDeleted, cancellationToken);

        if (existingCode)
            return Result<Guid>.Failure("Customer code already exists");

        // Check if email already exists
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var existingEmail = await _context.Customers
                .AnyAsync(c => c.OrganizationId == organizationId &&
                              c.Email == request.Email.ToLowerInvariant() &&
                              !c.IsDeleted, cancellationToken);

            if (existingEmail)
                return Result<Guid>.Failure("Customer email already exists");
        }

        // Parse customer type
        if (!Enum.TryParse<CustomerType>(request.Type, true, out var customerType))
            customerType = CustomerType.Individual;

        // Create customer
        var customer = Customer.Create(
            organizationId,
            request.CustomerCode,
            request.CustomerName,
            customerType,
            request.TaxId,
            request.Email,
            request.Phone);

        // Update billing address
        if (!string.IsNullOrWhiteSpace(request.BillingAddress))
        {
            customer.UpdateBillingAddress(
                request.BillingAddress,
                request.BillingCity,
                request.BillingCountry,
                request.BillingPostalCode);
        }

        // Set credit limit
        if (request.CreditLimit.HasValue)
            customer.SetCreditLimit(request.CreditLimit);

        // Set payment term
        if (request.PaymentTermId.HasValue)
            customer.SetPaymentTerm(request.PaymentTermId.Value);

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(customer.Id);
    }
}
