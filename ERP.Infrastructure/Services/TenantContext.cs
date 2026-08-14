using ERP.Application.Common.Interfaces;

namespace ERP.Infrastructure.Services;

/// <summary>
/// Implementation of ITenantContext that wraps ICurrentUserService.
/// </summary>
public class TenantContext : ITenantContext
{
	private readonly ICurrentUserService _currentUserService;

	public TenantContext(ICurrentUserService currentUserService)
	{
		_currentUserService = currentUserService;
	}

	public Guid? TenantId => _currentUserService.OrganizationId;

	public bool HasTenant => _currentUserService.OrganizationId.HasValue;
}
