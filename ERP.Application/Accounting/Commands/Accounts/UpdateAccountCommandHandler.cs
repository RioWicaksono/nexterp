using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Accounting.Entities;

namespace ERP.Application.Accounting.Commands.Accounts;

/// <summary>
/// Handler for UpdateAccountCommand
/// </summary>
public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateAccountCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.OrganizationId == null)
            return Result<bool>.Failure("User is not associated with an organization");

        var organizationId = _currentUser.OrganizationId.Value;

        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.Id &&
                                     a.OrganizationId == organizationId &&
                                     !a.IsDeleted, cancellationToken);

        if (account == null)
            return Result<bool>.Failure("Account not found");

        // Check if account code already exists for another account
        if (!string.IsNullOrWhiteSpace(request.AccountCode))
        {
            var existingCode = await _context.Accounts
                .AnyAsync(a => a.OrganizationId == organizationId &&
                              a.AccountCode == request.AccountCode.ToUpperInvariant() &&
                              a.Id != request.Id &&
                              !a.IsDeleted, cancellationToken);

            if (existingCode)
                return Result<bool>.Failure("Account code already exists");
        }

        // Validate parent account if provided
        if (request.ParentId.HasValue)
        {
            if (request.ParentId.Value == request.Id)
                return Result<bool>.Failure("Account cannot be its own parent");

            var parentExists = await _context.Accounts
                .AnyAsync(a => a.Id == request.ParentId.Value &&
                              a.OrganizationId == organizationId &&
                              !a.IsDeleted, cancellationToken);

            if (!parentExists)
                return Result<bool>.Failure("Parent account not found");
        }

        // Update basic properties using the Update method
        account.Update(request.Name, request.Description);

        // Update parent
        account.SetParent(request.ParentId);

        // Update bank/cash flags
        if (request.IsBankAccount)
        {
            account.SetAsBankAccount(request.BankAccountNumber, request.BankName);
        }
        else if (request.IsCashAccount)
        {
            account.SetAsCashAccount();
        }

        // Update active status
        if (request.IsActive)
            account.Activate();
        else
            account.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
