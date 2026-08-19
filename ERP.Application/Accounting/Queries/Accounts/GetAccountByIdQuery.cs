using MediatR;
using ERP.Application.Common.Base;

namespace ERP.Application.Accounting.Queries.Accounts;

/// <summary>
/// Query to get a single account by ID
/// </summary>
public class GetAccountByIdQuery : IRequest<Result<object>>
{
    public Guid Id { get; set; }

    public GetAccountByIdQuery(Guid id)
    {
        Id = id;
    }
}
