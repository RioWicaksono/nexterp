using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Accounting.Entities;

namespace ERP.Application.Accounting.Commands.Accounts;

/// <summary>
/// Handler for CreateAccountCommand
/// </summary>
public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateAccountCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.OrganizationId == null)
            return Result<Guid>.Failure("User is not associated with an organization");

        var organizationId = _currentUser.OrganizationId.Value;

        // Check if account code already exists
        if (!string.IsNullOrWhiteSpace(request.AccountCode))
        {
            var existingCode = await _context.Accounts
                .AnyAsync(a => a.OrganizationId == organizationId &&
                              a.AccountCode == request.AccountCode.ToUpperInvariant() &&
                              !a.IsDeleted, cancellationToken);

            if (existingCode)
                return Result<Guid>.Failure("Account code already exists");
        }

        // Validate parent account if provided
        if (request.ParentId.HasValue)
        {
            var parentExists = await _context.Accounts
                .AnyAsync(a => a.Id == request.ParentId.Value &&
                              a.OrganizationId == organizationId &&
                              !a.IsDeleted, cancellationToken);

            if (!parentExists)
                return Result<Guid>.Failure("Parent account not found");
        }

        // Create account using factory method
        var account = Account.Create(
            organizationId,
            request.AccountCode.ToUpperInvariant(),
            request.Name,
            request.AccountType,
            request.Class,
            request.Description,
            request.ParentId);

        // Set bank/cash flags if applicable
        if (request.IsBankAccount)
        {
            account.SetAsBankAccount(request.BankAccountNumber, request.BankName);
        }
        else if (request.IsCashAccount)
        {
            account.SetAsCashAccount();
        }

        // Set opening balance if provided
        if (request.OpeningBalance.HasValue)
        {
            var balanceDate = request.OpeningBalanceDate ?? DateTime.UtcNow;
            account.SetOpeningBalance(request.OpeningBalance.Value, balanceDate);
        }

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(account.Id);
    }
}
