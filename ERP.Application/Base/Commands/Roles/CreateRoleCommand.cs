using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Base;

namespace ERP.Application.Base.Commands.Roles;

public class CreateRoleCommand : ICommand<Guid>
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string>? Permissions { get; set; }
}

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty().WithMessage("Organization ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required")
            .MaximumLength(100).WithMessage("Role name cannot exceed 100 characters");
    }
}

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var organizationExists = await _context.Organizations
            .AnyAsync(o => o.Id == request.OrganizationId, cancellationToken);

        if (!organizationExists)
            return Result<Guid>.Failure("Organization not found");

        var existingRole = await _context.Roles
            .AnyAsync(r => r.OrganizationId == request.OrganizationId &&
                          r.Name.ToLower() == request.Name.ToLower(), cancellationToken);

        if (existingRole)
            return Result<Guid>.Failure("Role with this name already exists");

        var role = Role.Create(request.OrganizationId, request.Name, request.Description);

        if (request.Permissions?.Any() == true)
        {
            foreach (var permission in request.Permissions)
            {
                role.AddPermission(permission);
            }
        }

        _context.Roles.Add(role);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(role.Id);
    }
}
