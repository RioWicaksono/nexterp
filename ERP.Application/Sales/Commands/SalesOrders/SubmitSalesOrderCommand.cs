using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Sales.Entities;

namespace ERP.Application.Sales.Commands.SalesOrders;

/// <summary>
/// Command to submit a sales order for approval
/// </summary>
public class SubmitSalesOrderCommand : ICommand<bool>
{
    public Guid Id { get; set; }
}

/// <summary>
/// Handler for SubmitSalesOrderCommand
/// </summary>
public class SubmitSalesOrderCommandHandler : IRequestHandler<SubmitSalesOrderCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SubmitSalesOrderCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(SubmitSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUser.OrganizationId
            ?? throw new UnauthorizedAccessException("User is not associated with an organization");

        var order = await _context.SalesOrders
            .FirstOrDefaultAsync(o => o.Id == request.Id && !o.IsDeleted && o.OrganizationId == organizationId, cancellationToken);

        if (order == null)
            return Result<bool>.Failure("Sales order not found");

        try
        {
            order.Submit();
            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (InvalidOperationException ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}
