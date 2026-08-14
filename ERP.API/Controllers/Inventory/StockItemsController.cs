using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

using ERP.API.Controllers.Base;
using ERP.Application.Inventory.Commands.StockItems;
using ERP.Application.Inventory.DTOs;
using ERP.Application.Inventory.Queries;

namespace ERP.API.Controllers.Inventory;

/// <summary>
/// Stock item management endpoints
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/stock-items")]
[Authorize]
public class StockItemsController : BaseApiController
{
    private readonly IMediator _mediator;

    public StockItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all stock items with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StockItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] Guid? warehouseId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetStockItemsQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get stock item by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<StockItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStockItemByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new stock item
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateStockItemDto request, CancellationToken cancellationToken)
    {
        var command = new CreateStockItemCommand
        {
            Name = request.Name,
            Code = request.Code,
            Barcode = request.Barcode,
            Description = request.Description,
            CategoryId = request.CategoryId,
            UnitOfMeasureId = request.UnitOfMeasureId,
            ReorderLevel = request.ReorderLevel,
            MinimumStock = request.MinimumStock,
            MaximumStock = request.MaximumStock,
            StandardCost = request.StandardCost,
            StandardPrice = request.StandardPrice,
            ValuationMethod = request.ValuationMethod,
            TrackSerials = request.TrackSerials,
            TrackBatch = request.TrackBatch
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Created($"api/v1/stock-items/{result.Value}", result);

        return HandleResult(result);
    }

    /// <summary>
    /// Update stock item
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateStockItemDto request, CancellationToken cancellationToken)
    {
        // TODO: Implement update command
        return Error("Not implemented yet", StatusCodes.Status501NotImplemented);
    }

    /// <summary>
    /// Delete stock item
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        // TODO: Implement delete command
        return Error("Not implemented yet", StatusCodes.Status501NotImplemented);
    }

    /// <summary>
    /// Get stock balance for item
    /// </summary>
    [HttpGet("{id:guid}/stock-balance")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStockBalance(Guid id, CancellationToken cancellationToken)
    {
        // TODO: Implement query
        return Error("Not implemented yet", StatusCodes.Status501NotImplemented);
    }
}
