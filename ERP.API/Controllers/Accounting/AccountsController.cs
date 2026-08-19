using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

using ERP.API.Controllers.Base;
using ERP.Application.Accounting.Commands.Accounts;
using ERP.Application.Accounting.DTOs;
using ERP.Application.Accounting.Queries.Accounts;

namespace ERP.API.Controllers.Accounting;

/// <summary>
/// Account management endpoints
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/accounts")]
[Authorize]
public class AccountsController : BaseApiController
{
    private readonly IMediator _mediator;

    public AccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all accounts with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? accountType,
        [FromQuery] string? accountClass,
        [FromQuery] bool? isActive,
        [FromQuery] Guid? parentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? orderBy = "AccountCode",
        [FromQuery] bool orderByDescending = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAccountsQuery
        {
            Pagination = new ERP.Application.Common.DTOs.PaginationParams
            {
                Page = page,
                PageSize = pageSize,
                OrderBy = orderBy,
                OrderByDescending = orderByDescending
            },
            Search = search,
            ParentId = parentId,
            IsActive = isActive
        };

        // Parse account type filter
        if (!string.IsNullOrWhiteSpace(accountType) &&
            Enum.TryParse<ERP.Domain.Accounting.Enums.AccountType>(accountType, true, out var accType))
        {
            query.AccountType = accType;
        }

        // Parse account class filter
        if (!string.IsNullOrWhiteSpace(accountClass) &&
            Enum.TryParse<ERP.Domain.Accounting.Enums.AccountClass>(accountClass, true, out var accClass))
        {
            query.AccountClass = accClass;
        }

        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get account by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAccountByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new account
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAccountRequestDto request, CancellationToken cancellationToken)
    {
        var command = new CreateAccountCommand
        {
            AccountCode = request.AccountCode,
            Name = request.Name,
            Description = request.Description,
            AccountType = request.AccountType,
            Class = request.Class,
            ParentId = request.ParentId,
            IsBankAccount = request.IsBankAccount,
            IsCashAccount = request.IsCashAccount,
            OpeningBalance = request.OpeningBalance,
            OpeningBalanceDate = request.OpeningBalanceDate,
            BankAccountNumber = request.BankAccountNumber,
            BankName = request.BankName
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Created($"api/v1/accounts/{result.Value}", result);

        return HandleResult(result);
    }

    /// <summary>
    /// Update an existing account
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccountRequestDto request, CancellationToken cancellationToken)
    {
        var command = new UpdateAccountCommand
        {
            Id = id,
            AccountCode = request.AccountCode,
            Name = request.Name,
            Description = request.Description,
            AccountType = request.AccountType,
            Class = request.Class,
            ParentId = request.ParentId,
            IsBankAccount = request.IsBankAccount,
            IsCashAccount = request.IsCashAccount,
            IsActive = request.IsActive,
            BankAccountNumber = request.BankAccountNumber,
            BankName = request.BankName
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete an account (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteAccountCommand { Id = id }, cancellationToken);
        return HandleResult(result);
    }
}

/// <summary>
/// DTO for creating an account (request)
/// </summary>
public class CreateAccountRequestDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ERP.Domain.Accounting.Enums.AccountType AccountType { get; set; }
    public ERP.Domain.Accounting.Enums.AccountClass Class { get; set; }
    public Guid? ParentId { get; set; }
    public bool IsBankAccount { get; set; }
    public bool IsCashAccount { get; set; }
    public decimal? OpeningBalance { get; set; }
    public DateTime? OpeningBalanceDate { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
}

/// <summary>
/// DTO for updating an account (request)
/// </summary>
public class UpdateAccountRequestDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ERP.Domain.Accounting.Enums.AccountType AccountType { get; set; }
    public ERP.Domain.Accounting.Enums.AccountClass Class { get; set; }
    public Guid? ParentId { get; set; }
    public bool IsBankAccount { get; set; }
    public bool IsCashAccount { get; set; }
    public bool IsActive { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
}
