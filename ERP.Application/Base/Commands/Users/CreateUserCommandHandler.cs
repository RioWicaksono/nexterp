using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Base;

namespace ERP.Application.Base.Commands.Users;

/// <summary>
/// Handler for CreateUserCommand
/// </summary>
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Check if username already exists
        var existingUsername = await _context.Users
            .AnyAsync(u => u.Username == request.Username.ToLowerInvariant(), cancellationToken);

        if (existingUsername)
            return Result<Guid>.Failure("Username already exists");

        // Check if email already exists
        var existingEmail = await _context.Users
            .AnyAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

        if (existingEmail)
            return Result<Guid>.Failure("Email already exists");

        // Check if organization exists
        var organizationExists = await _context.Organizations
            .AnyAsync(o => o.Id == request.OrganizationId && !o.IsDeleted, cancellationToken);

        if (!organizationExists)
            return Result<Guid>.Failure("Organization not found");

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Create user
        var user = User.Create(
            request.OrganizationId,
            request.Username,
            request.Email,
            passwordHash,
            request.FirstName,
            request.LastName,
            request.Phone);

        _context.Users.Add(user);

        // Assign roles if provided
        if (request.RoleIds != null && request.RoleIds.Any())
        {
            var validRoles = await _context.Roles
                .Where(r => request.RoleIds.Contains(r.Id) && !r.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var role in validRoles)
            {
                user.AssignRole(UserRole.Create(user.Id, role.Id));
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(user.Id);
    }
}
