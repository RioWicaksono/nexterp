using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Analytics.Queries;

public class GetAuditLogsQuery : IRequest<Result<object>>
{
    public Guid? UserId { get; set; }
}

public class GetAuditLogsHandler : IRequestHandler<GetAuditLogsQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    public GetAuditLogsHandler(IApplicationDbContext ctx) { _ctx = ctx; }
    public async Task<Result<object>> Handle(GetAuditLogsQuery req, CancellationToken ct)
    {
        var query = _ctx.AuditLogs.AsNoTracking();
        if (req.UserId.HasValue)
            query = query.Where(l => l.UserId == req.UserId);
        var logs = await query.OrderByDescending(l => l.CreatedAt).Take(100).ToListAsync(ct);
        return Result<object>.Success(new { Items = logs.Select(l => new { l.Id, l.Module, l.Action, l.CreatedAt })});
    }
}

public class GetNotificationsQuery : IRequest<Result<object>>
{
    public Guid UserId { get; set; }
}

public class GetNotificationsHandler : IRequestHandler<GetNotificationsQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    public GetNotificationsHandler(IApplicationDbContext ctx) { _ctx = ctx; }
    public async Task<Result<object>> Handle(GetNotificationsQuery req, CancellationToken ct)
    {
        var notifications = await _ctx.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == req.UserId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync(ct);
        return Result<object>.Success(new { Items = notifications.Select(n => new { n.Id, n.Title, n.IsRead })});
    }
}
