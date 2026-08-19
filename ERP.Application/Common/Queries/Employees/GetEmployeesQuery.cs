using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.DTOs;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Common.Queries.Employees;

public class GetEmployeesQuery : IQuery<PaginatedResult<EmployeeSimpleDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
}

public class EmployeeSimpleDto
{
    public Guid Id { get; set; }
    public string? EmployeeNumber { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Department { get; set; }
    public string? Status { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? HireDate { get; set; }
}

public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, Result<PaginatedResult<EmployeeSimpleDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetEmployeesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<EmployeeSimpleDto>>> Handle(
        GetEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _context.Employees.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(e =>
                (e.EmployeeNumber != null && e.EmployeeNumber.ToLower().Contains(search)) ||
                e.FirstName.ToLower().Contains(search) ||
                (e.LastName != null && e.LastName.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(e => e.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EmployeeSimpleDto
            {
                Id = e.Id,
                EmployeeNumber = e.EmployeeNumber,
                FirstName = e.FirstName,
                LastName = e.LastName ?? string.Empty,
                Status = e.Status.ToString(),
                IsActive = e.Status == Domain.Hrm.Enums.EmployeeStatus.Active,
                HireDate = e.HireDate
            })
            .ToListAsync(cancellationToken);

        var result = new PaginatedResult<EmployeeSimpleDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return Result<PaginatedResult<EmployeeSimpleDto>>.Success(result);
    }
}
