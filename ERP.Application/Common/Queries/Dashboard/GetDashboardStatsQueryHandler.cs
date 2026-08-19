using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.DTOs.Dashboard;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Common.Queries.Dashboard;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DashboardStatsDto>> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var dto = new DashboardStatsDto
        {
            TotalEmployees = await _context.Employees.CountAsync(cancellationToken),
            TotalInventoryItems = await _context.StockItems.CountAsync(cancellationToken),
            TotalPurchaseOrders = await _context.PurchaseOrders.CountAsync(cancellationToken),
            TotalSuppliers = await _context.Suppliers.CountAsync(cancellationToken),
            TotalProjects = await _context.Projects.CountAsync(cancellationToken),
            TotalAccounts = await _context.Accounts.CountAsync(cancellationToken),
            RecentActivities = new List<ActivityDto>()
        };

        return Result<DashboardStatsDto>.Success(dto);
    }
}
