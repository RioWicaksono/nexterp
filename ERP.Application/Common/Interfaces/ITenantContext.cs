using ERP.Domain.Common.Modules;

namespace ERP.Application.Common.Interfaces;

/// <summary>
/// Service to get the current tenant ID for query filtering.
/// This is scoped to the HTTP request context.
/// </summary>
public interface ITenantContext
{
	/// <summary>
	/// Gets the current tenant ID, or null if not in a tenant context.
	/// </summary>
	Guid? TenantId { get; }

	/// <summary>
	/// Whether we're currently in a tenant context.
	/// </summary>
	bool HasTenant { get; }
}
