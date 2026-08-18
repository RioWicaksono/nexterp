using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Base.Commands.Roles;

public class DeleteRoleCommand : ICommand<bool>
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
}

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public DeleteRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == request.Id &&
                                    r.OrganizationId == request.OrganizationId, cancellationToken);

        if (role == null)
            return Result<bool>.Failure("Role not found");

        if (role.IsSystemRole)
            return Result<bool>.Failure("Cannot delete system role");

        var hasUsers = await _context.UserRoles
            .AnyAsync(ur => ur.RoleId == request.Id && !ur.IsDeleted, cancellationToken);

        if (hasUsers)
            return Result<bool>.Failure("Cannot delete role that is assigned to users. Remove all user assignments first.");

        // Soft delete the role
        role.MarkAsDeleted();
        role.Deactivate();

        // Remove all role permissions
        foreach (var permission in role.Permissions.ToList())
        {
            permission.MarkAsDeleted();
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
