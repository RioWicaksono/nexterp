using ERP.Application.Common.Interfaces;

namespace ERP.Application.Common.Base;

/// <summary>
/// Helper class for checking organization context in command handlers.
/// </summary>
public static class OrganizationContext
{
	/// <summary>
	/// Gets the organization ID from the current user service, or returns a failure result if not available.
	/// </summary>
	public static Guid? GetOrganizationIdOrNull(ICurrentUserService currentUser)
	{
		return currentUser.OrganizationId;
	}

	/// <summary>
	/// Validates that the current user has an organization context.
	/// Returns the organization ID if valid, null if not.
	/// </summary>
	public static Guid? ValidateOrganizationId(ICurrentUserService currentUser)
	{
		return currentUser.OrganizationId;
	}
}
