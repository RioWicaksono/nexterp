using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;

using ERP.API.Controllers.Base;
using ERP.Application.Base.Commands.Auth;
using ERP.Application.Base.DTOs;
using ERP.Application.Common.Configuration;
using ERP.Application.Common.Interfaces;
using ERP.Infrastructure.Services;
using ApplicationDbContext = ERP.Infrastructure.Persistence.ERPDbContext;
using OrganizationEntity = ERP.Domain.Base.Organization;
using UserEntity = ERP.Domain.Base.User;
using BaseEntity = ERP.Domain.Common.BaseEntity;

namespace ERP.API.Controllers.Auth;

/// <summary>
/// Authentication endpoints - Production ready with token security
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly JwtSettings _jwtSettings;
    private readonly ILoginRateLimitService _loginRateLimitService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IMediator mediator,
        IApplicationDbContext context,
        IJwtService jwtService,
        JwtSettings jwtSettings,
        ILoginRateLimitService loginRateLimitService,
        ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _context = context;
        _jwtService = jwtService;
        _jwtSettings = jwtSettings;
        _loginRateLimitService = loginRateLimitService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user and get tokens
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login([FromBody] LoginDto request, CancellationToken cancellationToken)
    {
        var clientIp = GetClientIp();

        // Check brute force protection
        var rateLimitResult = await _loginRateLimitService.CheckLoginAttemptAsync(
            clientIp, request.Username, isFailedAttempt: true, cancellationToken);

        if (rateLimitResult.IsLocked)
        {
            _logger.LogWarning(
                "Login blocked due to brute force protection for IP {IpAddress}, username {Username}",
                clientIp, request.Username);

            Response.Headers["Retry-After"] = rateLimitResult.RetryAfterSeconds.ToString();
            return StatusCode(429, new
            {
                success = false,
                error = "Too many login attempts. Please try again later.",
                retryAfterSeconds = rateLimitResult.RetryAfterSeconds
            });
        }

        var command = new LoginCommand
        {
            Username = request.Username,
            Password = request.Password
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess && result.Value != null)
        {
            // Clear failed login attempts on success
            await _loginRateLimitService.ClearLoginAttemptsAsync(clientIp, request.Username, cancellationToken);

            // Set httpOnly cookie for access token (production security)
            var accessCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                Path = "/"
            };

            Response.Cookies.Append("nexterp_token", result.Value.AccessToken, accessCookieOptions);

            // Set httpOnly cookie for refresh token
            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                Path = "/"
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

        // Hash password with BCrypt cost factor 12
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 12);

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
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        // Get refresh token from cookie or body
        var refreshToken = Request.Cookies["nexterp_refresh"];

        if (string.IsNullOrEmpty(refreshToken))
        {
            return BadRequest(new { error = "Refresh token required" });
        }

        var ctx = (ApplicationDbContext)_context;

        // Find user by refresh token (using stored hash)
        var user = await ctx.Users
            .FirstOrDefaultAsync(u => !u.IsDeleted, cancellationToken);

        if (user == null || !user.ValidateRefreshToken(refreshToken))
        {
            return Unauthorized(new { error = "Invalid or expired refresh token" });
        }

        // Check if account is locked or inactive
        if (user.IsLocked)
            return Unauthorized(new { error = "Account is locked" });

        if (!user.IsActive && !user.IsSuperAdmin)
            return Unauthorized(new { error = "Account is inactive" });

        // Load roles and permissions
        var userRoleIds = await ctx.UserRoles
            .Where(ur => ur.UserId == user.Id && !ur.IsDeleted)
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

        var userRoles = await ctx.Roles
            .Where(r => userRoleIds.Contains(r.Id) && !r.IsDeleted)
            .Select(r => r.Name)
            .ToListAsync(cancellationToken);

        if (user.IsSuperAdmin && !userRoles.Contains("SuperAdmin"))
            userRoles.Add("SuperAdmin");

        var rolePermissions = await ctx.RolePermissions
            .Where(rp => userRoleIds.Contains(rp.RoleId) && !rp.IsDeleted)
            .Select(rp => rp.Permission)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Generate new access token
        var (accessToken, expiresAt) = _jwtService.GenerateAccessToken(user, rolePermissions);

        // Rotate refresh token (invalidate old, generate new)
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        user.SetRefreshToken(newRefreshToken, TimeSpan.FromDays(_jwtSettings.RefreshTokenExpirationDays));
        await ctx.SaveChangesAsync(cancellationToken);

        // Set new access token cookie
        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            Path = "/"
        };
        Response.Cookies.Append("nexterp_token", accessToken, accessCookieOptions);

        // Set new refresh token cookie
        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            Path = "/"
        };
        Response.Cookies.Append("nexterp_refresh", newRefreshToken, refreshCookieOptions);

        return Ok(new
        {
            accessToken,
            refreshToken = newRefreshToken,
            expiresAt,
            tokenType = "Bearer"
        });
    }

    /// <summary>
    /// Logout - clear auth cookies and blacklist tokens
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var accessToken = Request.Cookies["nexterp_token"];

        if (!string.IsNullOrEmpty(accessToken))
        {
            // Blacklist the access token until it expires
            var expiry = _jwtSettings.AccessTokenExpirationMinutes * 60;
            await _jwtService.BlacklistTokenAsync(accessToken, TimeSpan.FromSeconds(expiry));
        }

        // Clear auth cookies
        Response.Cookies.Delete("nexterp_token", new CookieOptions { Path = "/" });
        Response.Cookies.Delete("nexterp_refresh", new CookieOptions { Path = "/" });

        return Ok(new { message = "Logged out successfully" });
    }

    private string GetClientIp()
    {
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
            return forwardedFor.Split(',')[0].Trim();

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
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
