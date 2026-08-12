using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Application.Assets.Commands;
using ERP.API.Controllers.Base;

namespace ERP.API.Controllers.Assets;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssetsController : BaseApiController
{
    private readonly IMediator _mediator;

    public AssetsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<AssetResponse>>> GetAll(
        [FromQuery] string? searchTerm,
        [FromQuery] string? status,
        [FromQuery] string? assetType)
    {
        var query = new GetAssetsQuery(searchTerm, status, assetType);
        var assets = await _mediator.Send(query);

        return Ok(assets.Select(a => new AssetResponse(
            a.Id, a.OrganizationId, a.AssetCode, a.Name, a.AssetType,
            a.ParentAssetId, a.PurchaseCost, a.PurchaseDate,
            a.WarrantyExpiry, a.Status, a.Notes
        )));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AssetResponse>> GetById(Guid id)
    {
        var asset = await _mediator.Send(new GetAssetByIdQuery(id));
        if (asset == null) return NotFound();

        return Ok(new AssetResponse(
            asset.Id, asset.OrganizationId, asset.AssetCode, asset.Name, asset.AssetType,
            asset.ParentAssetId, asset.PurchaseCost, asset.PurchaseDate,
            asset.WarrantyExpiry, asset.Status, asset.Notes
        ));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateAssetRequest request)
    {
        var command = new CreateAssetCommand(
            request.AssetCode,
            request.Name,
            request.AssetType,
            request.PurchaseCost,
            request.PurchaseDate,
            request.WarrantyExpiry,
            request.Notes
        );

        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<bool>> Update(Guid id, [FromBody] UpdateAssetRequest request)
    {
        var command = new UpdateAssetCommand(id, request.Name, request.AssetType, request.Status, request.Notes);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("maintenance")]
    public async Task<ActionResult<Guid>> CreateMaintenance([FromBody] CreateMaintenanceRequest request)
    {
        var command = new CreateAssetMaintenanceCommand(
            request.AssetId,
            request.Type,
            request.ScheduledDate,
            request.Cost,
            request.Notes
        );

        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(CreateMaintenance), new { id }, id);
    }
}

public record AssetResponse(
    Guid Id,
    Guid OrganizationId,
    string AssetCode,
    string Name,
    string AssetType,
    Guid? ParentAssetId,
    decimal PurchaseCost,
    DateTime? PurchaseDate,
    DateTime? WarrantyExpiry,
    string Status,
    string? Notes
);

public record CreateAssetRequest(
    string AssetCode,
    string Name,
    string AssetType,
    decimal PurchaseCost,
    DateTime? PurchaseDate,
    DateTime? WarrantyExpiry,
    string? Notes
);

public record UpdateAssetRequest(
    string Name,
    string AssetType,
    string Status,
    string? Notes
);

public record CreateMaintenanceRequest(
    Guid AssetId,
    string Type,
    DateTime ScheduledDate,
    decimal Cost,
    string? Notes
);
