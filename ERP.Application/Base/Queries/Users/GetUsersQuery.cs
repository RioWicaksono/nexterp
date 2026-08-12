using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.DTOs;
using ERP.Application.Common.Interfaces;
using ERP.Application.Base.DTOs;

namespace ERP.Application.Base.Queries.Users;

/// <summary>
/// Query to get user by ID
/// </summary>
public class GetUserByIdQuery : IQuery<UserDto>
{
    public Guid Id { get; set; }
}

/// <summary>
/// Query to get paginated users
/// </summary>
public class GetUsersPaginatedQuery : IQuery<PaginatedResult<UserDto>>
{
    public Guid? OrganizationId { get; set; }
    public bool? IsActive { get; set; }
    public string? Search { get; set; }
    public PaginationParams Pagination { get; set; } = new();
}

/// <summary>
/// Handler for GetUserByIdQuery
/// </summary>
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUserByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.Id && !u.IsDeleted, cancellationToken);

        if (user == null)
            return Result<UserDto>.Failure("User not found");

        var dto = new UserDto
        {
            Id = user.Id,
            OrganizationId = user.OrganizationId,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.Phone,
            IsActive = user.IsActive,
            IsSuperAdmin = user.IsSuperAdmin,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Roles = new List<string>()
        };

        return Result<UserDto>.Success(dto);
    }
}

/// <summary>
/// Handler for GetUsersPaginatedQuery
/// </summary>
public class GetUsersPaginatedQueryHandler : IRequestHandler<GetUsersPaginatedQuery, Result<PaginatedResult<UserDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersPaginatedQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<UserDto>>> Handle(GetUsersPaginatedQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users.AsNoTracking().Where(u => !u.IsDeleted);

        if (request.OrganizationId.HasValue)
            query = query.Where(u => u.OrganizationId == request.OrganizationId.Value);

        if (request.IsActive.HasValue)
            query = query.Where(u => u.IsActive == request.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(u =>
                u.Username.ToLower().Contains(search) ||
                u.Email.ToLower().Contains(search) ||
                u.FirstName.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderBy(u => u.Username)
            .Skip((request.Pagination.Page - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize)
            .ToListAsync(cancellationToken);

        var items = users.Select(user => new UserDto
        {
            Id = user.Id,
            OrganizationId = user.OrganizationId,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.Phone,
            IsActive = user.IsActive,
            IsSuperAdmin = user.IsSuperAdmin,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Roles = new List<string>()
        }).ToList();

        var result = new PaginatedResult<UserDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Pagination.Page,
            PageSize = request.Pagination.PageSize
        };

        return Result<PaginatedResult<UserDto>>.Success(result);
    }
}
