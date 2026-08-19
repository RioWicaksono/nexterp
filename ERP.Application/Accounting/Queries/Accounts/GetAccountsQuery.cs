using MediatR;
using ERP.Application.Common.Base;
using ERP.Application.Common.DTOs;
using ERP.Application.Common.Models;
using ERP.Domain.Accounting.Enums;

namespace ERP.Application.Accounting.Queries.Accounts;

/// <summary>
/// Query to get paginated list of accounts
/// </summary>
public class GetAccountsQuery : IQuery<PaginatedList<object>>
{
    public PaginationParams Pagination { get; set; } = new();
    public string? Search { get; set; }
    public AccountType? AccountType { get; set; }
    public AccountClass? AccountClass { get; set; }
    public bool? IsActive { get; set; }
    public Guid? ParentId { get; set; }
}
