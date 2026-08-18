using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Base.Commands.Roles;

public class AddRolePermissionsCommand : ICommand<bool>
{
    public Guid RoleId { get; set; }
    public Guid OrganizationId { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public class AddRolePermissionsCommandHandler : IRequestHandler<AddRolePermissionsCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public AddRolePermissionsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(AddRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == request.RoleId &&
                                    r.OrganizationId == request.OrganizationId, cancellationToken);

        if (role == null)
            return Result<bool>.Failure("Role not found");

        if (role.IsSystemRole)
            return Result<bool>.Failure("Cannot modify system role permissions");

        var existingPermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == request.RoleId && !rp.IsDeleted)
            .Select(rp => rp.Permission)
            .ToListAsync(cancellationToken);

        var addedPermissions = new List<string>();

        foreach (var permission in request.Permissions)
        {
            if (!existingPermissions.Contains(permission))
            {
                role.AddPermission(permission);
                addedPermissions.Add(permission);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
