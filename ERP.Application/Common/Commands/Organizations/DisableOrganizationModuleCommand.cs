using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Common.Commands.Organizations;

public class DisableOrganizationModuleCommand : ICommand<bool>
{
    public Guid OrganizationId { get; set; }
    public string ModuleCode { get; set; } = string.Empty;
}

public class DisableOrganizationModuleCommandHandler : IRequestHandler<DisableOrganizationModuleCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public DisableOrganizationModuleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(DisableOrganizationModuleCommand request, CancellationToken cancellationToken)
    {
        var orgModule = await _context.OrganizationModules
            .FirstOrDefaultAsync(om => om.OrganizationId == request.OrganizationId &&
                                 om.ModuleCode == request.ModuleCode.ToUpperInvariant() &&
                                 !om.IsDeleted, cancellationToken);

        if (orgModule == null)
            return Result<bool>.Failure("Module not enabled for organization");

        orgModule.MarkAsDeleted();
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
