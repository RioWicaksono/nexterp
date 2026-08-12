using ERP.Application.Common.Interfaces;
using ERP.Domain.Quality.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Quality.Commands;

public record CreateInspectionCommand(
    string Type,
    Guid? ReferenceId,
    string ReferenceType,
    DateTime InspectionDate,
    string? Inspector,
    string? Notes
) : IRequest<Guid>;

public class CreateInspectionHandler : IRequestHandler<CreateInspectionCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateInspectionHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateInspectionCommand request, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId ?? Guid.Empty;

        // Generate inspection number
        var count = await _context.Inspections
            .Where(i => i.OrganizationId == orgId)
            .CountAsync(cancellationToken);

        var inspection = new Inspection
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            InspectionNumber = $"INS-{DateTime.UtcNow:yyyyMM}-{count + 1:D4}",
            Type = request.Type,
            ReferenceId = request.ReferenceId,
            ReferenceType = request.ReferenceType,
            InspectionDate = request.InspectionDate,
            Status = "Pending",
            Inspector = request.Inspector,
            Notes = request.Notes
        };

        _context.Inspections.Add(inspection);
        await _context.SaveChangesAsync(cancellationToken);

        return inspection.Id;
    }
}

public record CompleteInspectionCommand(
    Guid Id,
    string Results,
    bool Passed,
    string? Notes
) : IRequest<bool>;

public class CompleteInspectionHandler : IRequestHandler<CompleteInspectionCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public CompleteInspectionHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(CompleteInspectionCommand request, CancellationToken cancellationToken)
    {
        var inspection = await _context.Inspections
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (inspection == null) return false;

        inspection.Results = request.Results;
        inspection.Passed = request.Passed;
        inspection.Status = request.Passed ? "Passed" : "Failed";
        inspection.Notes = request.Notes;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record CreateNonConformanceCommand(
    Guid? InspectionId,
    string Severity,
    string Description,
    string? RootCause,
    string? CorrectiveAction,
    string? PreventiveAction
) : IRequest<Guid>;

public class CreateNonConformanceHandler : IRequestHandler<CreateNonConformanceCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateNonConformanceHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateNonConformanceCommand request, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId ?? Guid.Empty;

        var nc = new NonConformance
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            InspectionId = request.InspectionId,
            Severity = request.Severity,
            Description = request.Description,
            RootCause = request.RootCause,
            CorrectiveAction = request.CorrectiveAction,
            PreventiveAction = request.PreventiveAction,
            Status = "Open"
        };

        _context.NonConformances.Add(nc);
        await _context.SaveChangesAsync(cancellationToken);

        return nc.Id;
    }
}

public record ResolveNonConformanceCommand(
    Guid Id,
    string? RootCause,
    string? CorrectiveAction,
    string? PreventiveAction
) : IRequest<bool>;

public class ResolveNonConformanceHandler : IRequestHandler<ResolveNonConformanceCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ResolveNonConformanceHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ResolveNonConformanceCommand request, CancellationToken cancellationToken)
    {
        var nc = await _context.NonConformances
            .FirstOrDefaultAsync(n => n.Id == request.Id, cancellationToken);

        if (nc == null) return false;

        nc.Status = "Resolved";
        nc.ResolvedAt = DateTime.UtcNow;
        nc.RootCause = request.RootCause ?? nc.RootCause;
        nc.CorrectiveAction = request.CorrectiveAction ?? nc.CorrectiveAction;
        nc.PreventiveAction = request.PreventiveAction ?? nc.PreventiveAction;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record GetInspectionsQuery(string? Status, string? Type) : IRequest<List<Inspection>>;

public class GetInspectionsHandler : IRequestHandler<GetInspectionsQuery, List<Inspection>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetInspectionsHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<Inspection>> Handle(GetInspectionsQuery request, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId ?? Guid.Empty;
        var query = _context.Inspections.Where(i => i.OrganizationId == orgId);

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(i => i.Status == request.Status);
        }

        if (!string.IsNullOrEmpty(request.Type))
        {
            query = query.Where(i => i.Type == request.Type);
        }

        return await query.OrderByDescending(i => i.InspectionDate).ToListAsync(cancellationToken);
    }
}

public record GetNonConformancesQuery(string? Status, string? Severity) : IRequest<List<NonConformance>>;

public class GetNonConformancesHandler : IRequestHandler<GetNonConformancesQuery, List<NonConformance>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetNonConformancesHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<NonConformance>> Handle(GetNonConformancesQuery request, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId ?? Guid.Empty;
        var query = _context.NonConformances.Where(n => n.OrganizationId == orgId);

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(n => n.Status == request.Status);
        }

        if (!string.IsNullOrEmpty(request.Severity))
        {
            query = query.Where(n => n.Severity == request.Severity);
        }

        return await query.OrderByDescending(n => n.CreatedAt).ToListAsync(cancellationToken);
    }
}
