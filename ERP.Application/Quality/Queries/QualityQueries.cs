using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Quality.Queries;

public class GetInspectionsQuery : IRequest<Result<object>>
{
    public Guid? OrganizationId { get; set; }
}

public class GetInspectionsHandler : IRequestHandler<GetInspectionsQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    public GetInspectionsHandler(IApplicationDbContext ctx) { _ctx = ctx; }
    public async Task<Result<object>> Handle(GetInspectionsQuery req, CancellationToken ct)
    {
        var query = _ctx.Inspections.AsNoTracking();
        if (req.OrganizationId.HasValue)
            query = query.Where(i => i.OrganizationId == req.OrganizationId);
        var inspections = await query.ToListAsync(ct);
        return Result<object>.Success(new { Items = inspections.Select(i => new { i.Id, i.Type, i.Status })});
    }
}

public class GetNonConformancesQuery : IRequest<Result<object>> { }

public class GetNonConformancesHandler : IRequestHandler<GetNonConformancesQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    public GetNonConformancesHandler(IApplicationDbContext ctx) { _ctx = ctx; }
    public async Task<Result<object>> Handle(GetNonConformancesQuery _, CancellationToken ct)
    {
        var ncs = await _ctx.NonConformances.AsNoTracking().ToListAsync(ct);
        return Result<object>.Success(new { Items = ncs.Select(n => new { n.Id, n.Severity, n.Status })});
    }
}
