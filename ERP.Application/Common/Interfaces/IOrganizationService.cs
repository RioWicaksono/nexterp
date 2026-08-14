namespace ERP.Application.Common.Interfaces;

/// <summary>
/// Organization service interface.
/// </summary>
public interface IOrganizationService
{
    /// <summary>
    /// Get organization name by ID.
    /// </summary>
    string GetName(Guid organizationId);

    /// <summary>
    /// Check if organization exists.
    /// </summary>
    Task<bool> ExistsAsync(Guid organizationId);
}
