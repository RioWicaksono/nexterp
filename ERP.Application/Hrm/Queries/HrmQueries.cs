using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Hrm.Entities;

namespace ERP.Application.Hrm.Queries;

public class GetEmployeesQuery : IRequest<Result<object>> { }

public class GetEmployeesHandler : IRequestHandler<GetEmployeesQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    public GetEmployeesHandler(IApplicationDbContext ctx) { _ctx = ctx; }
    public async Task<Result<object>> Handle(GetEmployeesQuery _, CancellationToken cancellationToken)
    {
        var emps = await _ctx.Employees.AsNoTracking().ToListAsync(cancellationToken);
        return Result<object>.Success(new { Items = emps.Select(e => new {
            e.Id,
            e.EmployeeNumber,
            FullName = e.FirstName + " " + (e.LastName ?? ""),
            Status = e.Status.ToString(),
            e.DepartmentId,
            e.PositionId
        })});
    }
}

public class GetEmployeeByIdQuery : IRequest<Result<object>>
{
    public Guid Id { get; set; }
    public GetEmployeeByIdQuery(Guid id) => Id = id;
}

public class GetEmployeeByIdHandler : IRequestHandler<GetEmployeeByIdQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    public GetEmployeeByIdHandler(IApplicationDbContext ctx) { _ctx = ctx; }
    public async Task<Result<object>> Handle(GetEmployeeByIdQuery req, CancellationToken cancellationToken)
    {
        var emp = await _ctx.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == req.Id, cancellationToken);

        if (emp == null)
            return Result<object>.Failure("Employee not found");

        return Result<object>.Success(new {
            emp.Id,
            emp.EmployeeNumber,
            FullName = emp.FirstName + " " + (emp.LastName ?? ""),
            emp.DateOfBirth,
            Gender = emp.Gender.ToString(),
            Status = emp.Status.ToString(),
            emp.DepartmentId,
            emp.PositionId,
            emp.HireDate
        });
    }
}

public class GetAttendancesQuery : IRequest<Result<object>> { }

public class GetAttendancesHandler : IRequestHandler<GetAttendancesQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    public GetAttendancesHandler(IApplicationDbContext ctx) { _ctx = ctx; }
    public async Task<Result<object>> Handle(GetAttendancesQuery _, CancellationToken cancellationToken)
    {
        var attendances = await _ctx.Attendances.AsNoTracking().ToListAsync(cancellationToken);
        return Result<object>.Success(new { Items = attendances.Select(a => new {
            a.Id,
            a.EmployeeId,
            a.Date,
            Status = a.Status.ToString(),
            a.CheckInTime,
            a.CheckOutTime
        })});
    }
}

public class GetAttendanceByIdQuery : IRequest<Result<object>>
{
    public Guid Id { get; set; }
    public GetAttendanceByIdQuery(Guid id) => Id = id;
}

public class GetAttendanceByIdHandler : IRequestHandler<GetAttendanceByIdQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    public GetAttendanceByIdHandler(IApplicationDbContext ctx) { _ctx = ctx; }
    public async Task<Result<object>> Handle(GetAttendanceByIdQuery req, CancellationToken cancellationToken)
    {
        var attendance = await _ctx.Attendances
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == req.Id, cancellationToken);

        if (attendance == null)
            return Result<object>.Failure("Attendance not found");

        return Result<object>.Success(new {
            attendance.Id,
            attendance.EmployeeId,
            attendance.Date,
            Status = attendance.Status.ToString(),
            attendance.CheckInTime,
            attendance.CheckOutTime
        });
    }
}

public class GetLeaveRequestsQuery : IRequest<Result<object>> { }

public class GetLeaveRequestsHandler : IRequestHandler<GetLeaveRequestsQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    public GetLeaveRequestsHandler(IApplicationDbContext ctx) { _ctx = ctx; }
    public async Task<Result<object>> Handle(GetLeaveRequestsQuery _, CancellationToken cancellationToken)
    {
        var leaves = await _ctx.LeaveRequests.AsNoTracking().ToListAsync(cancellationToken);
        return Result<object>.Success(new { Items = leaves.Select(l => new {
            l.Id,
            l.EmployeeId,
            LeaveType = l.LeaveType.ToString(),
            l.StartDate,
            l.EndDate,
            Status = l.Status.ToString()
        })});
    }
}

public class GetLeaveRequestByIdQuery : IRequest<Result<object>>
{
    public Guid Id { get; set; }
    public GetLeaveRequestByIdQuery(Guid id) => Id = id;
}

public class GetLeaveRequestByIdHandler : IRequestHandler<GetLeaveRequestByIdQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    public GetLeaveRequestByIdHandler(IApplicationDbContext ctx) { _ctx = ctx; }
    public async Task<Result<object>> Handle(GetLeaveRequestByIdQuery req, CancellationToken cancellationToken)
    {
        var leave = await _ctx.LeaveRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == req.Id, cancellationToken);

        if (leave == null)
            return Result<object>.Failure("Leave request not found");

        return Result<object>.Success(new {
            leave.Id,
            leave.EmployeeId,
            LeaveType = leave.LeaveType.ToString(),
            leave.StartDate,
            leave.EndDate,
            leave.TotalDays,
            Status = leave.Status.ToString(),
            leave.Reason
        });
    }
}
