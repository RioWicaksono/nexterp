using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Modules;

namespace ERP.Application.Common.Queries.Organizations;

public class GetOrganizationModulesQuery : IQuery<List<OrganizationModuleDto>>
{
    public Guid OrganizationId { get; set; }
}

public class OrganizationModuleDto
{
    public Guid Id { get; set; }
    public string ModuleCode { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? ActivatedBy { get; set; }
    public string Tier { get; set; } = string.Empty;
    public bool IsPremium { get; set; }
}

public class GetOrganizationModulesQueryHandler : IRequestHandler<GetOrganizationModulesQuery, Result<List<OrganizationModuleDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetOrganizationModulesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<OrganizationModuleDto>>> Handle(GetOrganizationModulesQuery request, CancellationToken cancellationToken)
    {
        // Get all available modules
        var allModules = ModuleConfigurationLoader.GetAllModules();

        // Get enabled modules for the organization
        var enabledModules = await _context.OrganizationModules
            .Where(om => om.OrganizationId == request.OrganizationId && !om.IsDeleted)
            .ToDictionaryAsync(om => om.ModuleCode.ToUpperInvariant(), cancellationToken);

        var result = new List<OrganizationModuleDto>();

        foreach (var moduleConfig in allModules)
        {
            var moduleCode = moduleConfig.Code.ToUpperInvariant();
            var isEnabled = enabledModules.ContainsKey(moduleCode);
            var orgModule = isEnabled ? enabledModules[moduleCode] : null;

            result.Add(new OrganizationModuleDto
            {
                Id = orgModule?.Id ?? Guid.Empty,
                ModuleCode = moduleConfig.Code,
                ModuleName = moduleConfig.Module,
                Description = moduleConfig.Settings.GetValueOrDefault("description")?.ToString(),
                IsEnabled = isEnabled,
                ActivatedAt = orgModule?.ActivatedAt,
                ExpiresAt = orgModule?.ExpiresAt,
                ActivatedBy = orgModule?.ActivatedBy,
                Tier = moduleConfig.Tier,
                IsPremium = moduleConfig.Settings.ContainsKey("premium")
            });
        }

        return Result<List<OrganizationModuleDto>>.Success(result);
    }
}
