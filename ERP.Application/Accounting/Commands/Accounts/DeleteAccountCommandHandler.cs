using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Accounting.Commands.Accounts;

/// <summary>
/// Handler for DeleteAccountCommand
/// </summary>
public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteAccountCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
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

        // Check if account has child accounts
        var hasChildren = await _context.Accounts
            .AnyAsync(a => a.ParentId == request.Id &&
                          a.OrganizationId == organizationId &&
                          !a.IsDeleted, cancellationToken);

        if (hasChildren)
            return Result<bool>.Failure("Cannot delete account with child accounts. Delete or reassign children first.");

        // Check if account has journal lines
        var hasJournalLines = await _context.JournalLines
            .AnyAsync(jl => jl.AccountId == request.Id && !jl.IsDeleted, cancellationToken);

        if (hasJournalLines)
            return Result<bool>.Failure("Cannot delete account with journal transactions. Consider deactivating instead.");

        // Soft delete the account
        account.MarkAsDeleted(_currentUser.Username);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
