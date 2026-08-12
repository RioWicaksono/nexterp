using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Inventory.Entities;

namespace ERP.Application.Inventory.Queries;

public class GetWarehousesQuery : IRequest<Result<object>> { }

public class GetWarehousesHandler : IRequestHandler<GetWarehousesQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    private readonly ICurrentUserService _currentUser;

    public GetWarehousesHandler(IApplicationDbContext ctx, ICurrentUserService currentUser)
    {
        _ctx = ctx;
        _currentUser = currentUser;
    }

    public async Task<Result<object>> Handle(GetWarehousesQuery _, CancellationToken ct)
    {
        var organizationId = _currentUser.OrganizationId;

        var warehouses = await _ctx.Warehouses
            .AsNoTracking()
            .Where(w => w.OrganizationId == organizationId)
            .ToListAsync(ct);

        return Result<object>.Success(new { Items = warehouses.Select(w => new { w.Id, w.Name, w.Code, w.IsActive })});
    }
}

public class GetWarehouseByIdQuery : IRequest<Result<object>>
{
    public Guid Id { get; set; }
    public GetWarehouseByIdQuery(Guid id) => Id = id;
}

public class GetWarehouseByIdHandler : IRequestHandler<GetWarehouseByIdQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    private readonly ICurrentUserService _currentUser;

    public GetWarehouseByIdHandler(IApplicationDbContext ctx, ICurrentUserService currentUser)
    {
        _ctx = ctx;
        _currentUser = currentUser;
    }

    public async Task<Result<object>> Handle(GetWarehouseByIdQuery req, CancellationToken ct)
    {
        var organizationId = _currentUser.OrganizationId;

        var warehouse = await _ctx.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == req.Id && w.OrganizationId == organizationId, ct);

        if (warehouse == null)
            return Result<object>.Failure("Warehouse not found or access denied");

        return Result<object>.Success(new { warehouse.Id, warehouse.Name, warehouse.Code, warehouse.IsActive });
    }
}

public class GetStockItemsQuery : IRequest<Result<object>> { }

public class GetStockItemsHandler : IRequestHandler<GetStockItemsQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    private readonly ICurrentUserService _currentUser;

    public GetStockItemsHandler(IApplicationDbContext ctx, ICurrentUserService currentUser)
    {
        _ctx = ctx;
        _currentUser = currentUser;
    }

    public async Task<Result<object>> Handle(GetStockItemsQuery _, CancellationToken ct)
    {
        var organizationId = _currentUser.OrganizationId;

        var items = await _ctx.StockItems
            .AsNoTracking()
            .Where(i => i.OrganizationId == organizationId)
            .ToListAsync(ct);

        return Result<object>.Success(new { Items = items.Select(i => new { i.Id, i.Name, i.Code, i.IsActive })});
    }
}

public class GetStockItemByIdQuery : IRequest<Result<object>>
{
    public Guid Id { get; set; }
    public GetStockItemByIdQuery(Guid id) => Id = id;
}

public class GetStockItemByIdHandler : IRequestHandler<GetStockItemByIdQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    private readonly ICurrentUserService _currentUser;

    public GetStockItemByIdHandler(IApplicationDbContext ctx, ICurrentUserService currentUser)
    {
        _ctx = ctx;
        _currentUser = currentUser;
    }

    public async Task<Result<object>> Handle(GetStockItemByIdQuery req, CancellationToken ct)
    {
        var organizationId = _currentUser.OrganizationId;

        var item = await _ctx.StockItems
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == req.Id && i.OrganizationId == organizationId, ct);

        if (item == null)
            return Result<object>.Failure("Stock item not found or access denied");

        return Result<object>.Success(new { item.Id, item.Name, item.Code, item.IsActive });
    }
}
