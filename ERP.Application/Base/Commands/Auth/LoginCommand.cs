using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Application.Base.DTOs;
using ERP.Domain.Base;

namespace ERP.Application.Base.Commands.Auth;

/// <summary>
/// Command to authenticate user
/// </summary>
public class LoginCommand : ICommand<LoginResponseDto>
{
	public string Username { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Validator for LoginCommand
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
	public LoginCommandValidator()
	{
		RuleFor(x => x.Username)
			.NotEmpty().WithMessage("Username is required");

		RuleFor(x => x.Password)
			.NotEmpty().WithMessage("Password is required");
	}
}

/// <summary>
/// Handler for LoginCommand
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
	private readonly IApplicationDbContext _context;
	private readonly IJwtService _jwtService;

	// Dummy password hash for timing attack prevention
	// This is used when user doesn't exist so we still perform hash verification
	private const string DummyPasswordHash = "$2a$11$KasandraSecurityDummyHashForTimingPrevention";

	public LoginCommandHandler(IApplicationDbContext context, IJwtService jwtService)
	{
		_context = context;
		_jwtService = jwtService;
	}

	public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
	{
		var normalizedUsername = request.Username.ToLowerInvariant().Trim();

		// Fetch user by username only (not email)
		var user = await _context.Users
			.AsNoTracking()
			.FirstOrDefaultAsync(u =>
				u.Username == normalizedUsername &&
				!u.IsDeleted,
				cancellationToken);

		// Use actual password hash if user exists, otherwise use dummy hash
		// This ensures password verification takes the same time regardless of user existence
		var passwordHashToVerify = user?.PasswordHash ?? DummyPasswordHash;

		// Always perform password verification (constant-time comparison via BCrypt)
		var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, passwordHashToVerify);

		// If user doesn't exist OR password is invalid, return same error
		// This prevents username enumeration via timing differences
		if (user == null || !isPasswordValid)
		{
			return Result<LoginResponseDto>.Failure("Invalid username or password");
		}

		// User exists and password is valid - now check account status
		// Check if account is locked
		if (user.IsLocked)
			return Result<LoginResponseDto>.Failure("Account is locked. Please try again later.");

		// Check if account is active (skip for super admin)
		if (!user.IsActive && !user.IsSuperAdmin)
			return Result<LoginResponseDto>.Failure("Account is inactive");

		// Load user roles
		var userRoleIds = await _context.UserRoles
			.Where(ur => ur.UserId == user.Id && !ur.IsDeleted)
			.Select(ur => ur.RoleId)
			.ToListAsync(cancellationToken);

		var userRoles = await _context.Roles
			.Where(r => userRoleIds.Contains(r.Id) && !r.IsDeleted)
			.Select(r => r.Name)
			.ToListAsync(cancellationToken);

		// Add SuperAdmin role if user is super admin
		if (user.IsSuperAdmin && !userRoles.Contains("SuperAdmin"))
		{
			userRoles.Add("SuperAdmin");
		}

		// Load role permissions
		var rolePermissions = await _context.RolePermissions
			.Where(rp => userRoleIds.Contains(rp.RoleId) && !rp.IsDeleted)
			.Select(rp => rp.Permission)
			.Distinct()
			.ToListAsync(cancellationToken);

		// Generate tokens with roles and permissions
		var (accessToken, expiresAt) = _jwtService.GenerateAccessToken(user, rolePermissions);

		// Add role claims to JWT
		var response = new LoginResponseDto
		{
			AccessToken = accessToken,
			RefreshToken = _jwtService.GenerateRefreshToken(),
			ExpiresAt = expiresAt,
			User = new UserDto
			{
				Id = user.Id,
				OrganizationId = user.OrganizationId,
				Username = user.Username,
				Email = user.Email,
				FirstName = user.FirstName,
				LastName = user.LastName,
				Phone = user.Phone,
				IsActive = user.IsActive,
				IsSuperAdmin = user.IsSuperAdmin,
				LastLoginAt = user.LastLoginAt,
				Roles = userRoles
			}
		};

		return Result<LoginResponseDto>.Success(response);
	}
}
