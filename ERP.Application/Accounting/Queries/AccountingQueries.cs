using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Accounting.Queries;

public class GetAccountsQuery : IRequest<Result<object>> { }

public class GetAccountsHandler : IRequestHandler<GetAccountsQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    public GetAccountsHandler(IApplicationDbContext ctx) { _ctx = ctx; }
    public async Task<Result<object>> Handle(GetAccountsQuery _, CancellationToken cancellationToken)
    {
        var accounts = await _ctx.Accounts.AsNoTracking().ToListAsync(cancellationToken);
        return Result<object>.Success(new { Items = accounts.Select(a => new {
            a.Id,
            a.Name,
            a.AccountCode,
            Type = a.Type.ToString(),
            a.IsActive
        })});
    }
}

public class GetJournalEntriesQuery : IRequest<Result<object>> { }

public class GetJournalEntriesHandler : IRequestHandler<GetJournalEntriesQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    public GetJournalEntriesHandler(IApplicationDbContext ctx) { _ctx = ctx; }
    public async Task<Result<object>> Handle(GetJournalEntriesQuery _, CancellationToken cancellationToken)
    {
        var entries = await _ctx.JournalEntries.AsNoTracking().ToListAsync(cancellationToken);
        return Result<object>.Success(new { Items = entries.Select(e => new {
            e.Id,
            e.EntryNumber,
            e.EntryDate,
            Status = e.Status.ToString(),
            e.TotalDebit,
            e.TotalCredit
        })});
    }
}

public class GetJournalEntryByIdQuery : IRequest<Result<object>>
{
    public Guid Id { get; set; }
    public GetJournalEntryByIdQuery(Guid id) => Id = id;
}

public class GetJournalEntryByIdHandler : IRequestHandler<GetJournalEntryByIdQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    public GetJournalEntryByIdHandler(IApplicationDbContext ctx) { _ctx = ctx; }
    public async Task<Result<object>> Handle(GetJournalEntryByIdQuery req, CancellationToken cancellationToken)
    {
        var entry = await _ctx.JournalEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == req.Id, cancellationToken);

        if (entry == null)
            return Result<object>.Failure("Journal entry not found");

        return Result<object>.Success(new {
            entry.Id,
            entry.EntryNumber,
            entry.EntryDate,
            Status = entry.Status.ToString(),
            entry.TotalDebit,
            entry.TotalCredit
        });
    }
}
