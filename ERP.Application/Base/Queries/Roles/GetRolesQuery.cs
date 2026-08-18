using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Common.DTOs;
using ERP.Domain.Base;

namespace ERP.Application.Base.Queries.Roles;

public class GetRolesQuery : IQuery<PaginatedList<RoleDto>>
{
    public Guid OrganizationId { get; set; }
    public bool? IsActive { get; set; }
    public string? Search { get; set; }
    public PaginationParams Pagination { get; set; } = new();
}

public class RoleDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool IsSystemRole { get; set; }
    public int UserCount { get; set; }
    public int PermissionCount { get; set; }
    public List<string> Permissions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, Result<PaginatedList<RoleDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetRolesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<RoleDto>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Roles
            .Where(r => r.OrganizationId == request.OrganizationId && !r.IsDeleted)
            .AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(r => r.IsActive == request.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.ToLower();
            query = query.Where(r => r.Name.ToLower().Contains(searchLower) ||
                                   (r.Description != null && r.Description.ToLower().Contains(searchLower)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var roles = await query
            .Include(r => r.Permissions.Where(p => !p.IsDeleted))
            .OrderBy(r => r.Name)
            .Skip((request.Pagination.Page - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize)
            .ToListAsync(cancellationToken);

        var roleDtos = new List<RoleDto>();

        foreach (var role in roles)
        {
            var userCount = await _context.UserRoles
                .CountAsync(ur => ur.RoleId == role.Id && !ur.IsDeleted, cancellationToken);

            roleDtos.Add(new RoleDto
            {
                Id = role.Id,
                OrganizationId = role.OrganizationId,
                Name = role.Name,
                Description = role.Description,
                IsActive = role.IsActive,
                IsSystemRole = role.IsSystemRole,
                UserCount = userCount,
                PermissionCount = role.Permissions.Count,
                Permissions = role.Permissions.Select(p => p.Permission).ToList(),
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt
            });
        }

        return Result<PaginatedList<RoleDto>>.Success(new PaginatedList<RoleDto>(
            roleDtos,
            totalCount,
            request.Pagination.Page,
            request.Pagination.PageSize
        ));
    }
}
