using Microsoft.AspNetCore.Mvc;
using ERP.Application.Common.Base;

namespace ERP.API.Controllers.Base;

/// <summary>
/// Base API controller with common functionality
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Returns success result with data
    /// </summary>
    protected IActionResult Success<T>(T data)
    {
        return Ok(new ApiResponse<T>
        {
            Success = true,
            Data = data
        });
    }

    /// <summary>
    /// Returns success result without data
    /// </summary>
    protected IActionResult Success()
    {
        return Ok(new ApiResponse
        {
            Success = true
        });
    }

    /// <summary>
    /// Returns created result with location header
    /// </summary>
    protected IActionResult Created<T>(string location, T data)
    {
        return Created(location, new ApiResponse<T>
        {
            Success = true,
            Data = data
        });
    }

    /// <summary>
    /// Returns error result
    /// </summary>
    protected IActionResult Error(string message, int statusCode = 400)
    {
        return StatusCode(statusCode, new ApiResponse
        {
            Success = false,
            Error = message
        });
    }

    /// <summary>
    /// Returns validation error result
    /// </summary>
    protected IActionResult ValidationError(string message)
    {
        return BadRequest(new ApiResponse
        {
            Success = false,
            Error = message
        });
    }

    /// <summary>
    /// Returns not found result
    /// </summary>
    protected IActionResult NotFoundError(string message = "Resource not found")
    {
        return NotFound(new ApiResponse
        {
            Success = false,
            Error = message
        });
    }

    /// <summary>
    /// Handles Result from CQRS commands/queries
    /// </summary>
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Success(result.Value);

        return Error(result.Error ?? "An error occurred");
    }

    /// <summary>
    /// Handles Result without value
    /// </summary>
    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return Success();

        return Error(result.Error ?? "An error occurred");
    }
}

/// <summary>
/// Standard API response wrapper
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// API response with data
/// </summary>
public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }
}
