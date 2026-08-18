using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Base.Commands.Users;

public class DeleteUserCommand : ICommand<bool>
{
    public Guid Id { get; set; }
}

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public DeleteUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id && !u.IsDeleted, cancellationToken);

        if (user == null)
            return Result<bool>.Failure("User not found");

        if (user.IsSuperAdmin)
            return Result<bool>.Failure("Cannot delete super admin user");

        user.MarkAsDeleted();
        user.Deactivate();

        // Soft delete all user roles
        var userRoles = await _context.UserRoles
            .Where(ur => ur.UserId == request.Id && !ur.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var role in userRoles)
        {
            role.MarkAsDeleted();
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
