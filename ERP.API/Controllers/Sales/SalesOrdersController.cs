using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.API.Controllers.Base;
using ERP.Application.Sales.Commands.SalesOrders;
using ERP.Application.Sales.DTOs;
using ERP.Application.Sales.Queries;

namespace ERP.API.Controllers.Sales;

/// <summary>
/// Sales Order management endpoints
/// </summary>
[ApiController]
[Route("api/v1/sales-orders")]
[Authorize]
public class SalesOrdersController : BaseApiController
{
    private readonly IMediator _mediator;

    public SalesOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all sales orders with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SalesOrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? customerId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetSalesOrdersQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get sales order by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSalesOrderByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new sales order
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSalesOrderDto request, CancellationToken cancellationToken)
    {
        var command = new CreateSalesOrderCommand
        {
            OrderDate = request.OrderDate,
            DeliveryDate = request.DeliveryDate,
            CustomerId = request.CustomerId,
            PriceListId = request.PriceListId,
            PaymentTermId = request.PaymentTermId,
            Notes = request.Notes,
            WarehouseId = request.WarehouseId,
            Lines = request.Lines
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Created($"api/v1/sales-orders/{result.Value}", result);

        return HandleResult(result);
    }

    /// <summary>
    /// Submit sales order
    /// </summary>
    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SubmitSalesOrderCommand { Id = id }, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Approve sales order
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ApproveSalesOrderCommand { Id = id }, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Cancel sales order
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelSalesOrderCommand { Id = id }, cancellationToken);
        return HandleResult(result);
    }
}
