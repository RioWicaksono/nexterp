using ERP.Application.Common.Interfaces;
using ERP.Domain.Assets.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Assets.Commands;

public record CreateAssetCommand(
    string AssetCode,
    string Name,
    string AssetType,
    decimal PurchaseCost,
    DateTime? PurchaseDate,
    DateTime? WarrantyExpiry,
    string? Notes
) : IRequest<Guid>;

public class CreateAssetHandler : IRequestHandler<CreateAssetCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateAssetHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateAssetCommand request, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId ?? Guid.Empty;

        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            AssetCode = request.AssetCode,
            Name = request.Name,
            AssetType = request.AssetType,
            PurchaseCost = request.PurchaseCost,
            PurchaseDate = request.PurchaseDate,
            WarrantyExpiry = request.WarrantyExpiry,
            Status = "Active",
            Notes = request.Notes
        };

        _context.Assets.Add(asset);
        await _context.SaveChangesAsync(cancellationToken);

        return asset.Id;
    }
}

public record UpdateAssetCommand(
    Guid Id,
    string Name,
    string AssetType,
    string Status,
    string? Notes
) : IRequest<bool>;

public class UpdateAssetHandler : IRequestHandler<UpdateAssetCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateAssetHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await _context.Assets.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        if (asset == null) return false;

        asset.Name = request.Name;
        asset.AssetType = request.AssetType;
        asset.Status = request.Status;
        asset.Notes = request.Notes;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record CreateAssetMaintenanceCommand(
    Guid AssetId,
    string Type,
    DateTime ScheduledDate,
    decimal Cost,
    string? Notes
) : IRequest<Guid>;

public class CreateAssetMaintenanceHandler : IRequestHandler<CreateAssetMaintenanceCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateAssetMaintenanceHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateAssetMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var maintenance = new AssetMaintenance
        {
            Id = Guid.NewGuid(),
            AssetId = request.AssetId,
            Type = request.Type,
            ScheduledDate = request.ScheduledDate,
            Status = "Scheduled",
            Cost = request.Cost,
            Notes = request.Notes
        };

        _context.AssetMaintenances.Add(maintenance);
        await _context.SaveChangesAsync(cancellationToken);

        return maintenance.Id;
    }
}

public record GetAssetsQuery(string? SearchTerm, string? Status, string? AssetType) : IRequest<List<Asset>>;

public class GetAssetsHandler : IRequestHandler<GetAssetsQuery, List<Asset>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAssetsHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<Asset>> Handle(GetAssetsQuery request, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId ?? Guid.Empty;
        var query = _context.Assets.Where(a => a.OrganizationId == orgId);

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(a =>
                a.Name.Contains(request.SearchTerm) ||
                a.AssetCode.Contains(request.SearchTerm));
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(a => a.Status == request.Status);
        }

        if (!string.IsNullOrEmpty(request.AssetType))
        {
            query = query.Where(a => a.AssetType == request.AssetType);
        }

        return await query.OrderBy(a => a.Name).ToListAsync(cancellationToken);
    }
}

public record GetAssetByIdQuery(Guid AssetId) : IRequest<Asset?>;

public class GetAssetByIdHandler : IRequestHandler<GetAssetByIdQuery, Asset?>
{
    private readonly IApplicationDbContext _context;

    public GetAssetByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Asset?> Handle(GetAssetByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Assets
            .Include(a => a.Id)
            .FirstOrDefaultAsync(a => a.Id == request.AssetId, cancellationToken);
    }
}
