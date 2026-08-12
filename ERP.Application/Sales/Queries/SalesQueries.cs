using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Sales.Entities;

namespace ERP.Application.Sales.Queries;

public class GetCustomersQuery : IRequest<Result<object>> { }

public class GetCustomersHandler : IRequestHandler<GetCustomersQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    public GetCustomersHandler(IApplicationDbContext ctx) { _ctx = ctx; }
    public async Task<Result<object>> Handle(GetCustomersQuery _, CancellationToken ct)
    {
        var customers = await _ctx.Customers.AsNoTracking().ToListAsync(ct);
        return Result<object>.Success(new { Items = customers.Select(c => new {
            c.Id,
            c.CustomerName,
            c.CustomerCode,
            c.Email,
            c.Phone,
            c.IsActive
        })});
    }
}

public class GetCustomerByIdQuery : IRequest<Result<object>>
{
    public Guid Id { get; set; }
    public GetCustomerByIdQuery(Guid id) => Id = id;
}

public class GetCustomerByIdHandler : IRequestHandler<GetCustomerByIdQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    public GetCustomerByIdHandler(IApplicationDbContext ctx) { _ctx = ctx; }
    public async Task<Result<object>> Handle(GetCustomerByIdQuery req, CancellationToken ct)
    {
        var customer = await _ctx.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == req.Id, ct);

        if (customer == null)
            return Result<object>.Failure("Customer not found");

        return Result<object>.Success(new {
            customer.Id,
            customer.CustomerName,
            customer.CustomerCode,
            customer.Email,
            customer.Phone,
            customer.IsActive
        });
    }
}

public class GetSalesOrdersQuery : IRequest<Result<object>> { }

public class GetSalesOrdersHandler : IRequestHandler<GetSalesOrdersQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    public GetSalesOrdersHandler(IApplicationDbContext ctx) { _ctx = ctx; }
    public async Task<Result<object>> Handle(GetSalesOrdersQuery _, CancellationToken ct)
    {
        var orders = await _ctx.SalesOrders.AsNoTracking().ToListAsync(ct);
        return Result<object>.Success(new { Items = orders.Select(o => new {
            o.Id,
            o.OrderNumber,
            o.OrderDate,
            o.CustomerId,
            Status = o.Status.ToString(),
            o.TotalAmount
        })});
    }
}

public class GetSalesOrderByIdQuery : IRequest<Result<object>>
{
    public Guid Id { get; set; }
    public GetSalesOrderByIdQuery(Guid id) => Id = id;
}

public class GetSalesOrderByIdHandler : IRequestHandler<GetSalesOrderByIdQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    public GetSalesOrderByIdHandler(IApplicationDbContext ctx) { _ctx = ctx; }
    public async Task<Result<object>> Handle(GetSalesOrderByIdQuery req, CancellationToken ct)
    {
        var order = await _ctx.SalesOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == req.Id, ct);

        if (order == null)
            return Result<object>.Failure("Sales order not found");

        return Result<object>.Success(new {
            order.Id,
            order.OrderNumber,
            order.OrderDate,
            order.CustomerId,
            Status = order.Status.ToString(),
            order.TotalAmount
        });
    }
}
