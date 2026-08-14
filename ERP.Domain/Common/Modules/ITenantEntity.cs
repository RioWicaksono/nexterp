namespace ERP.Domain.Common.Modules;

/// <summary>
/// Marker interface for entities that belong to an organization (tenant).
/// Used for automatic tenant filtering and organization ID population.
/// </summary>
public interface ITenantEntity
{
    Guid OrganizationId { get; }
}
