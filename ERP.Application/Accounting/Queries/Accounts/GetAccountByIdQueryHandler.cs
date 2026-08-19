using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Accounting.Queries.Accounts;

/// <summary>
/// Handler for GetAccountByIdQuery
/// </summary>
public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, Result<object>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAccountByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<object>> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.OrganizationId == null)
            return Result<object>.Failure("User is not associated with an organization");

        var organizationId = _currentUser.OrganizationId.Value;

        var account = await _context.Accounts
            .AsNoTracking()
            .Include(a => a.Parent)
            .Include(a => a.Children.Where(c => !c.IsDeleted))
            .FirstOrDefaultAsync(a => a.Id == request.Id &&
                                     a.OrganizationId == organizationId &&
                                     !a.IsDeleted, cancellationToken);

        if (account == null)
            return Result<object>.Failure("Account not found");

        var result = new
        {
            account.Id,
            account.OrganizationId,
            account.AccountCode,
            account.Name,
            account.Description,
            AccountType = account.Type.ToString(),
            Class = account.Class.ToString(),
            account.ParentId,
            ParentName = account.Parent?.Name,
            account.IsBankAccount,
            account.IsCashAccount,
            account.OpeningBalance,
            account.OpeningBalanceDate,
            account.BankAccountNumber,
            account.BankName,
            Balance = account.Balance,
            account.IsActive,
            account.AllowDirectPosting,
            account.CreatedAt,
            account.UpdatedAt,
            Children = account.Children.Select(c => new
            {
                c.Id,
                c.AccountCode,
                c.Name,
                c.IsActive
            })
        };

        return Result<object>.Success(result);
    }
}
