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
		// Always fetch user by username (or null if not found)
		var user = await _context.Users
			.AsNoTracking()
			.FirstOrDefaultAsync(u =>
				u.Username == request.Username.ToLowerInvariant() &&
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

		// Generate tokens
		var roles = new List<string>();
		var (accessToken, expiresAt) = _jwtService.GenerateAccessToken(user, roles);
		var refreshToken = _jwtService.GenerateRefreshToken();

		var response = new LoginResponseDto
		{
			AccessToken = accessToken,
			RefreshToken = refreshToken,
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
				Roles = roles
			}
		};

		return Result<LoginResponseDto>.Success(response);
	}
}
