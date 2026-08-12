using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Purchasing.Queries;

public class GetSuppliersQuery : IRequest<Result<object>> { }

public class GetSuppliersHandler : IRequestHandler<GetSuppliersQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    public GetSuppliersHandler(IApplicationDbContext ctx) { _ctx = ctx; }
    public async Task<Result<object>> Handle(GetSuppliersQuery _, CancellationToken cancellationToken)
    {
        var suppliers = await _ctx.Suppliers.AsNoTracking().ToListAsync(cancellationToken);
        return Result<object>.Success(new { Items = suppliers.Select(s => new {
            s.Id,
            s.SupplierName,
            s.SupplierCode,
            s.Email,
            s.Phone,
            s.IsActive
        })});
    }
}

public class GetSupplierByIdQuery : IRequest<Result<object>>
{
    public Guid Id { get; set; }
    public GetSupplierByIdQuery(Guid id) => Id = id;
}

public class GetSupplierByIdHandler : IRequestHandler<GetSupplierByIdQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    public GetSupplierByIdHandler(IApplicationDbContext ctx) { _ctx = ctx; }
    public async Task<Result<object>> Handle(GetSupplierByIdQuery req, CancellationToken cancellationToken)
    {
        var supplier = await _ctx.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == req.Id, cancellationToken);

        if (supplier == null)
            return Result<object>.Failure("Supplier not found");

        return Result<object>.Success(new {
            supplier.Id,
            supplier.SupplierName,
            supplier.SupplierCode,
            supplier.Email,
            supplier.Phone,
            supplier.IsActive
        });
    }
}

public class GetPurchaseOrdersQuery : IRequest<Result<object>> { }

public class GetPurchaseOrdersHandler : IRequestHandler<GetPurchaseOrdersQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    public GetPurchaseOrdersHandler(IApplicationDbContext ctx) { _ctx = ctx; }
    public async Task<Result<object>> Handle(GetPurchaseOrdersQuery _, CancellationToken cancellationToken)
    {
        var orders = await _ctx.PurchaseOrders.AsNoTracking().ToListAsync(cancellationToken);
        return Result<object>.Success(new { Items = orders.Select(o => new {
            o.Id,
            o.OrderNumber,
            o.OrderDate,
            o.SupplierId,
            Status = o.Status.ToString(),
            o.TotalAmount
        })});
    }
}

public class GetPurchaseOrderByIdQuery : IRequest<Result<object>>
{
    public Guid Id { get; set; }
    public GetPurchaseOrderByIdQuery(Guid id) => Id = id;
}

public class GetPurchaseOrderByIdHandler : IRequestHandler<GetPurchaseOrderByIdQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    public GetPurchaseOrderByIdHandler(IApplicationDbContext ctx) { _ctx = ctx; }
    public async Task<Result<object>> Handle(GetPurchaseOrderByIdQuery req, CancellationToken cancellationToken)
    {
        var order = await _ctx.PurchaseOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == req.Id, cancellationToken);

        if (order == null)
            return Result<object>.Failure("Purchase order not found");

        return Result<object>.Success(new {
            order.Id,
            order.OrderNumber,
            order.OrderDate,
            order.SupplierId,
            Status = order.Status.ToString(),
            order.TotalAmount
        });
    }
}
