using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Projects.Queries;

public class GetProjectsQuery : IRequest<Result<object>> { }

public class GetProjectsHandler : IRequestHandler<GetProjectsQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    public GetProjectsHandler(IApplicationDbContext ctx) { _ctx = ctx; }
    public async Task<Result<object>> Handle(GetProjectsQuery _, CancellationToken cancellationToken)
    {
        var projects = await _ctx.Projects.AsNoTracking().ToListAsync(cancellationToken);
        return Result<object>.Success(new { Items = projects.Select(p => new {
            p.Id,
            p.Name,
            p.Code,
            p.StartDate,
            p.EndDate,
            Status = p.Status.ToString()
        })});
    }
}

public class GetProjectByIdQuery : IRequest<Result<object>>
{
    public Guid Id { get; set; }
    public GetProjectByIdQuery(Guid id) => Id = id;
}

public class GetProjectByIdHandler : IRequestHandler<GetProjectByIdQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    public GetProjectByIdHandler(IApplicationDbContext ctx) { _ctx = ctx; }
    public async Task<Result<object>> Handle(GetProjectByIdQuery req, CancellationToken cancellationToken)
    {
        var project = await _ctx.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == req.Id, cancellationToken);

        if (project == null)
            return Result<object>.Failure("Project not found");

        return Result<object>.Success(new {
            project.Id,
            project.Name,
            project.Code,
            project.StartDate,
            project.EndDate,
            Status = project.Status.ToString()
        });
    }
}

public class GetProjectTasksQuery : IRequest<Result<object>> { }

public class GetProjectTasksHandler : IRequestHandler<GetProjectTasksQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    public GetProjectTasksHandler(IApplicationDbContext ctx) { _ctx = ctx; }
    public async Task<Result<object>> Handle(GetProjectTasksQuery _, CancellationToken cancellationToken)
    {
        var tasks = await _ctx.ProjectTasks.AsNoTracking().ToListAsync(cancellationToken);
        return Result<object>.Success(new { Items = tasks.Select(t => new {
            t.Id,
            t.Title,
            t.ProjectId,
            Status = t.Status.ToString(),
            t.StartDate,
            t.DueDate
        })});
    }
}

public class GetProjectTaskByIdQuery : IRequest<Result<object>>
{
    public Guid Id { get; set; }
    public GetProjectTaskByIdQuery(Guid id) => Id = id;
}

public class GetProjectTaskByIdHandler : IRequestHandler<GetProjectTaskByIdQuery, Result<object>>
{
    private readonly IApplicationDbContext _ctx;
    public GetProjectTaskByIdHandler(IApplicationDbContext ctx) { _ctx = ctx; }
    public async Task<Result<object>> Handle(GetProjectTaskByIdQuery req, CancellationToken cancellationToken)
    {
        var task = await _ctx.ProjectTasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == req.Id, cancellationToken);

        if (task == null)
            return Result<object>.Failure("Project task not found");

        return Result<object>.Success(new {
            task.Id,
            task.Title,
            task.ProjectId,
            Status = task.Status.ToString(),
            task.StartDate,
            task.DueDate
        });
    }
}
