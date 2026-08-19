using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Domain.Accounting.Entities;

namespace ERP.Application.Accounting.Queries.Accounts;

/// <summary>
/// Handler for GetAccountsQuery
/// </summary>
public class GetAccountsQueryHandler : IRequestHandler<GetAccountsQuery, Result<PaginatedList<object>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAccountsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<PaginatedList<object>>> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.OrganizationId == null)
            return Result<PaginatedList<object>>.Failure("User is not associated with an organization");

        var organizationId = _currentUser.OrganizationId.Value;

        var query = _context.Accounts
            .AsNoTracking()
            .Where(a => a.OrganizationId == organizationId && !a.IsDeleted);

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.ToLower();
            query = query.Where(a => a.AccountCode.ToLower().Contains(searchTerm) ||
                                     a.Name.ToLower().Contains(searchTerm));
        }

        if (request.AccountType.HasValue)
        {
            query = query.Where(a => a.Type == request.AccountType.Value);
        }

        if (request.AccountClass.HasValue)
        {
            query = query.Where(a => a.Class == request.AccountClass.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(a => a.IsActive == request.IsActive.Value);
        }

        if (request.ParentId.HasValue)
        {
            query = query.Where(a => a.ParentId == request.ParentId.Value);
        }
        else if (request.ParentId == null && request.Search == null &&
                 request.AccountType == null && request.AccountClass == null)
        {
            // Only show root accounts (no parent) by default if no specific filters
            query = query.Where(a => a.ParentId == null);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        var orderBy = request.Pagination.OrderBy?.ToLower() ?? "accountcode";
        query = orderBy switch
        {
            "name" => request.Pagination.OrderByDescending
                ? query.OrderByDescending(a => a.Name)
                : query.OrderBy(a => a.Name),
            "type" => request.Pagination.OrderByDescending
                ? query.OrderByDescending(a => a.Type)
                : query.OrderBy(a => a.Type),
            "class" => request.Pagination.OrderByDescending
                ? query.OrderByDescending(a => a.Class)
                : query.OrderBy(a => a.Class),
            "balance" => request.Pagination.OrderByDescending
                ? query.OrderByDescending(a => a.Balance)
                : query.OrderBy(a => a.Balance),
            "createdat" => request.Pagination.OrderByDescending
                ? query.OrderByDescending(a => a.CreatedAt)
                : query.OrderBy(a => a.CreatedAt),
            _ => request.Pagination.OrderByDescending
                ? query.OrderByDescending(a => a.AccountCode)
                : query.OrderBy(a => a.AccountCode)
        };

        // Apply pagination
        var page = request.Pagination.Page < 1 ? 1 : request.Pagination.Page;
        var pageSize = request.Pagination.PageSize < 1 ? 10 : request.Pagination.PageSize;

        var accounts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var accountDtos = accounts.Select(a => (object)new
        {
            a.Id,
            a.OrganizationId,
            a.AccountCode,
            a.Name,
            a.Description,
            AccountType = a.Type.ToString(),
            Class = a.Class.ToString(),
            a.ParentId,
            a.IsBankAccount,
            a.IsCashAccount,
            a.OpeningBalance,
            Balance = a.Balance,
            a.IsActive,
            a.AllowDirectPosting,
            a.CreatedAt,
            a.UpdatedAt
        }).ToList();

        var result = new PaginatedList<object>(accountDtos, totalCount, page, pageSize);

        return Result<PaginatedList<object>>.Success(result);
    }
}
