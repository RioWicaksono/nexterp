using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Purchasing.Entities;

namespace ERP.Application.Purchasing.Commands.PurchaseOrders;

/// <summary>
/// Command to approve a submitted purchase order
/// </summary>
public class ApprovePurchaseOrderCommand : ICommand<bool>
{
    public Guid Id { get; set; }
}

/// <summary>
/// Handler for ApprovePurchaseOrderCommand
/// </summary>
public class ApprovePurchaseOrderCommandHandler : IRequestHandler<ApprovePurchaseOrderCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ApprovePurchaseOrderCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(ApprovePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUser.OrganizationId
            ?? throw new UnauthorizedAccessException("User is not associated with an organization");

        var order = await _context.PurchaseOrders
            .FirstOrDefaultAsync(o => o.Id == request.Id && !o.IsDeleted && o.OrganizationId == organizationId, cancellationToken);

        if (order == null)
            return Result<bool>.Failure("Purchase order not found");

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
