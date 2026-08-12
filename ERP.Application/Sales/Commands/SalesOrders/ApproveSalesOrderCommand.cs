using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Sales.Entities;

namespace ERP.Application.Sales.Commands.SalesOrders;

/// <summary>
/// Command to approve a submitted sales order
/// </summary>
public class ApproveSalesOrderCommand : ICommand<bool>
{
    public Guid Id { get; set; }
}

/// <summary>
/// Handler for ApproveSalesOrderCommand
/// </summary>
public class ApproveSalesOrderCommandHandler : IRequestHandler<ApproveSalesOrderCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ApproveSalesOrderCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(ApproveSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUser.OrganizationId
            ?? throw new UnauthorizedAccessException("User is not associated with an organization");

        var order = await _context.SalesOrders
            .FirstOrDefaultAsync(o => o.Id == request.Id && !o.IsDeleted && o.OrganizationId == organizationId, cancellationToken);

        if (order == null)
            return Result<bool>.Failure("Sales order not found");

        try
        {
            order.Approve();
            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (InvalidOperationException ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}
