using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

using ERP.API.Controllers.Base;
using ERP.Application.Sales.Commands.Customers;
using ERP.Application.Sales.DTOs;
using ERP.Application.Sales.Queries;

namespace ERP.API.Controllers.Sales;

/// <summary>
/// Customer management endpoints
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/customers")]
[Authorize]
public class CustomersController : BaseApiController
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all customers with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CustomerDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetCustomersQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCustomerByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto request, CancellationToken cancellationToken)
    {
        var command = new CreateCustomerCommand
        {
            CustomerCode = request.CustomerCode,
            CustomerName = request.CustomerName,
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
            return Created($"api/v1/customers/{result.Value}", new ApiResponse<Guid> { Success = true, Data = result.Value });
        return HandleResult(result);
    }

    /// <summary>
    /// Update customer
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateCustomerDto request, CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerCommand
        {
            Id = id,
            CustomerCode = request.CustomerCode,
            CustomerName = request.CustomerName,
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
            PaymentTermId = request.PaymentTermId,
            IsActive = true
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete customer
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteCustomerCommand { Id = id }, cancellationToken);
        return HandleResult(result);
    }
}
