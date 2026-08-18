using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Base.Queries.Users;

public class GetUserRolesQuery : IQuery<List<UserRoleDto>>
{
    public Guid UserId { get; set; }
    public Guid OrganizationId { get; set; }
}

public class UserRoleDto
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? RoleDescription { get; set; }
    public bool IsSystemRole { get; set; }
    public DateTime AssignedAt { get; set; }
}

public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, Result<List<UserRoleDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetUserRolesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<UserRoleDto>>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        var userRoles = await _context.UserRoles
            .Where(ur => ur.UserId == request.UserId && !ur.IsDeleted)
            .Join(_context.Roles.Where(r => !r.IsDeleted),
                  ur => ur.RoleId,
                  r => r.Id,
                  (ur, r) => new { UserRole = ur, Role = r })
            .ToListAsync(cancellationToken);

        var result = userRoles.Select(x => new UserRoleDto
        {
            Id = x.UserRole.Id,
            RoleId = x.Role.Id,
            RoleName = x.Role.Name,
            RoleDescription = x.Role.Description,
            IsSystemRole = x.Role.IsSystemRole,
            AssignedAt = x.UserRole.CreatedAt
        }).ToList();

        return Result<List<UserRoleDto>>.Success(result);
    }
}
