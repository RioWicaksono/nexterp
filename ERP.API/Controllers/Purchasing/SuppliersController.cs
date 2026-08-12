using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.API.Controllers.Base;
using ERP.Application.Purchasing.Commands.Suppliers;
using ERP.Application.Purchasing.DTOs;
using ERP.Application.Purchasing.Queries;

namespace ERP.API.Controllers.Purchasing;

/// <summary>
/// Supplier management endpoints
/// </summary>
[ApiController]
[Route("api/v1/suppliers")]
[Authorize]
public class SuppliersController : BaseApiController
{
    private readonly IMediator _mediator;

    public SuppliersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all suppliers with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SupplierDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetSuppliersQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get supplier by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSupplierByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new supplier
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSupplierDto request, CancellationToken cancellationToken)
    {
        var command = new CreateSupplierCommand
        {
            SupplierCode = request.SupplierCode,
            SupplierName = request.SupplierName,
            Type = request.Type,
            TaxId = request.TaxId,
            Email = request.Email,
            Phone = request.Phone,
            Mobile = request.Mobile,
            BillingAddress = request.BillingAddress,
            BillingCity = request.BillingCity,
            BillingCountry = request.BillingCountry,
            BillingPostalCode = request.BillingPostalCode,
            CreditLimit = request.CreditLimit,
            PaymentTermId = request.PaymentTermId
        };

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsSuccess)
            return Created($"api/v1/suppliers/{result.Value}", new ApiResponse<Guid> { Success = true, Data = result.Value });
        return HandleResult(result);
    }

    /// <summary>
    /// Update supplier
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateSupplierDto request, CancellationToken cancellationToken)
    {
        var command = new UpdateSupplierCommand
        {
            Id = id,
            SupplierCode = request.SupplierCode,
            SupplierName = request.SupplierName,
            Type = request.Type,
            TaxId = request.TaxId,
            Email = request.Email,
            Phone = request.Phone,
            Mobile = request.Mobile,
            BillingAddress = request.BillingAddress,
            BillingCity = request.BillingCity,
            BillingCountry = request.BillingCountry,
            BillingPostalCode = request.BillingPostalCode,
            CreditLimit = request.CreditLimit,
            IsActive = true
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete supplier
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteSupplierCommand { Id = id }, cancellationToken);
        return HandleResult(result);
    }
}
