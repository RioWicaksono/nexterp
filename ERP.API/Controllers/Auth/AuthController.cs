using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

using Microsoft.EntityFrameworkCore;
using ERP.API.Controllers.Base;
using ERP.Application.Base.Commands.Auth;
using ERP.Application.Base.DTOs;
using ERP.Application.Common.Interfaces;
using ERP.Infrastructure.Services;
using ApplicationDbContext = ERP.Infrastructure.Persistence.ERPDbContext;
using OrganizationEntity = ERP.Domain.Base.Organization;
using UserEntity = ERP.Domain.Base.User;
using BaseEntity = ERP.Domain.Common.BaseEntity;

namespace ERP.API.Controllers.Auth;

/// <summary>
/// Authentication endpoints
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthController(IMediator mediator, IApplicationDbContext context, IJwtService jwtService)
    {
        _mediator = mediator;
        _context = context;
        _jwtService = jwtService;
    }

    /// <summary>
    /// Authenticate user and get tokens
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand
        {
            Username = request.Username,
            Password = request.Password
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess && result.Value != null)
        {
            // Set httpOnly cookie for access token
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            };

            Response.Cookies.Append("nexterp_token", result.Value.AccessToken, cookieOptions);

            // Set httpOnly cookie for refresh token
            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("nexterp_refresh", result.Value.RefreshToken, refreshCookieOptions);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Register new user and organization
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto request, CancellationToken cancellationToken)
    {
        var ctx = (ApplicationDbContext)_context;

        // Check if email already exists
        var existingUser = await ctx.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (existingUser != null)
        {
            return BadRequest(new { error = "Email already registered" });
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Create organization
        var orgId = Guid.NewGuid();
        var organization = OrganizationEntity.Create(request.OrganizationName ?? "My Organization");
        organization.Activate();
        var orgProp = typeof(BaseEntity).GetProperty("Id");
        orgProp!.SetValue(organization, orgId);
        ctx.Organizations.Add(organization);

        // Create user
        var userId = Guid.NewGuid();
        var user = UserEntity.Create(orgId, request.Username, request.Email, passwordHash, request.FirstName ?? "", request.LastName ?? "");
        user.Activate();
        var userProp = typeof(BaseEntity).GetProperty("Id");
        userProp!.SetValue(user, userId);
        ctx.Users.Add(user);

        await ctx.SaveChangesAsync(cancellationToken);

        return StatusCode(201, new RegisterResponseDto
        {
            UserId = userId,
            OrganizationId = orgId,
            Message = "Registration successful"
        });
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public IActionResult Refresh([FromBody] RefreshTokenDto request)
    {
        return BadRequest(new { error = "Not implemented" });
    }

    /// <summary>
    /// Logout - clear auth cookies
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // Clear auth cookies
        Response.Cookies.Delete("nexterp_token");
        Response.Cookies.Delete("nexterp_refresh");

        return Ok(new { message = "Logged out successfully" });
    }
}

/// <summary>
/// Registration DTO
/// </summary>
public class RegisterDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? OrganizationName { get; set; }
}

/// <summary>
/// Registration response DTO
/// </summary>
public class RegisterResponseDto
{
    public Guid UserId { get; set; }
    public Guid OrganizationId { get; set; }
    public string Message { get; set; } = string.Empty;
}
