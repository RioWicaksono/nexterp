namespace ERP.Domain.Base.Interfaces;

/// <summary>
/// Marker interface for all entities in the domain
/// </summary>
public interface IEntity
{
    Guid Id { get; }
    bool IsDeleted { get; }
}
