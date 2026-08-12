using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Purchasing.Commands.Suppliers;

/// <summary>
/// Command to delete a supplier (soft delete)
/// </summary>
public class DeleteSupplierCommand : ICommand<bool>
{
    public Guid Id { get; set; }
}

/// <summary>
/// Handler for DeleteSupplierCommand
/// </summary>
public class DeleteSupplierCommandHandler : IRequestHandler<DeleteSupplierCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteSupplierCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUser.OrganizationId
            ?? throw new UnauthorizedAccessException("User is not associated with an organization");

        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == request.Id && !s.IsDeleted && s.OrganizationId == organizationId, cancellationToken);

        if (supplier == null)
            return Result<bool>.Failure("Supplier not found");

        supplier.MarkAsDeleted();
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
