using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

using ERP.API.Controllers.Base;
using ERP.Application.Inventory.Commands.Warehouses;
using ERP.Application.Inventory.DTOs;
using ERP.Application.Inventory.Queries;

namespace ERP.API.Controllers.Inventory;

/// <summary>
/// Warehouse management endpoints
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class WarehousesController : BaseApiController
{
    private readonly IMediator _mediator;

    public WarehousesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all warehouses
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<WarehouseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWarehousesQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get warehouse by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<WarehouseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWarehouseByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new warehouse
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseDto request, CancellationToken cancellationToken)
    {
        var command = new CreateWarehouseCommand
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            Address = request.Address,
            City = request.City,
            Country = request.Country,
            Phone = request.Phone,
            Email = request.Email,
            IsDefault = request.IsDefault,
            AllowsNegativeStock = request.AllowsNegativeStock
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Created($"api/v1/warehouses/{result.Value}", result);

        return HandleResult(result);
    }

    /// <summary>
    /// Update warehouse
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateWarehouseDto request, CancellationToken cancellationToken)
    {
        // TODO: Implement update command
        return Error("Not implemented yet", StatusCodes.Status501NotImplemented);
    }

    /// <summary>
    /// Delete warehouse
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        // TODO: Implement delete command
        return Error("Not implemented yet", StatusCodes.Status501NotImplemented);
    }
}
