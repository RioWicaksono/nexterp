using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Base.Commands.Roles;

public class RemoveRolePermissionsCommand : ICommand<bool>
{
    public Guid RoleId { get; set; }
    public Guid OrganizationId { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public class RemoveRolePermissionsCommandHandler : IRequestHandler<RemoveRolePermissionsCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public RemoveRolePermissionsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(RemoveRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == request.RoleId &&
                                    r.OrganizationId == request.OrganizationId, cancellationToken);

        if (role == null)
            return Result<bool>.Failure("Role not found");

        if (role.IsSystemRole)
            return Result<bool>.Failure("Cannot modify system role permissions");

        var rolePermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == request.RoleId &&
                        request.Permissions.Contains(rp.Permission) &&
                        !rp.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var permission in rolePermissions)
        {
            permission.MarkAsDeleted();
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
