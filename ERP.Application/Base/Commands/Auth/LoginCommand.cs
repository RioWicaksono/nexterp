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

    public LoginCommandHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.Username == request.Username.ToLowerInvariant() &&
                !u.IsDeleted,
                cancellationToken);

        if (user == null)
            return Result<LoginResponseDto>.Failure("Invalid username or password");

        // Check if account is locked
        if (user.IsLocked)
            return Result<LoginResponseDto>.Failure("Account is locked. Please try again later.");

        // Check if account is active (skip for super admin)
        if (!user.IsActive && !user.IsSuperAdmin)
            return Result<LoginResponseDto>.Failure("Account is inactive");

        // Verify password
        if (string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Result<LoginResponseDto>.Failure("Invalid username or password");

        // Generate tokens with empty roles for now
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
