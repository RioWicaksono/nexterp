using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Base;

namespace ERP.Application.Base.Commands.Roles;

public class UpdateRoleCommand : ICommand<bool>
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Role ID is required");

        RuleFor(x => x.OrganizationId)
            .NotEmpty().WithMessage("Organization ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required")
            .MaximumLength(100).WithMessage("Role name cannot exceed 100 characters");
    }
}

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public UpdateRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == request.Id &&
                                    r.OrganizationId == request.OrganizationId, cancellationToken);

        if (role == null)
            return Result<bool>.Failure("Role not found");

        if (role.IsSystemRole)
            return Result<bool>.Failure("Cannot modify system role");

        var existingRole = await _context.Roles
            .AnyAsync(r => r.OrganizationId == request.OrganizationId &&
                          r.Name.ToLower() == request.Name.ToLower() &&
                          r.Id != request.Id, cancellationToken);

        if (existingRole)
            return Result<bool>.Failure("Role with this name already exists");

        role.Update(request.Name, request.Description);

        if (request.IsActive)
            role.Activate();
        else
            role.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
