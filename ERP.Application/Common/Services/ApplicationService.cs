using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.DTOs;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Common.Services;

/// <summary>
/// Base service for common CRUD operations
/// </summary>
public abstract class ApplicationService
{
    protected readonly IApplicationDbContext Context;
    protected readonly ICurrentUserService CurrentUser;

    protected ApplicationService(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        Context = context;
        CurrentUser = currentUser;
    }

    /// <summary>
    /// Get current user's organization ID
    /// </summary>
    protected Guid OrganizationId => CurrentUser.OrganizationId ?? throw new UnauthorizedAccessException("User is not associated with an organization");

    /// <summary>
    /// Get current user's ID
    /// </summary>
    protected Guid UserId => CurrentUser.UserId ?? throw new UnauthorizedAccessException("User is not authenticated");
}

/// <summary>
/// Generic service for CRUD operations
/// </summary>
public abstract class CrudService<TEntity, TDto, TCreateDto, TUpdateDto>
    where TEntity : class
    where TDto : BaseDto
    where TCreateDto : class
    where TUpdateDto : class
{
    protected readonly IApplicationDbContext Context;

    protected CrudService(IApplicationDbContext context)
    {
        Context = context;
    }

    public virtual async Task<TDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Context.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public virtual async Task<IEnumerable<TDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await Context.Set<TEntity>().ToListAsync(cancellationToken);
        return entities.Select(MapToDto);
    }

    public virtual async Task<TDto> CreateAsync(TCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = MapToEntity(dto);
        Context.Set<TEntity>().Add(entity);
        await Context.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public virtual async Task<TDto> UpdateAsync(Guid id, TUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await Context.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken);
        if (entity == null)
            throw new KeyNotFoundException($"Entity with ID {id} not found");

        MapToEntity(dto, entity);
        await Context.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Context.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken);
        if (entity == null)
            throw new KeyNotFoundException($"Entity with ID {id} not found");

        Context.Set<TEntity>().Remove(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }

    protected abstract TDto MapToDto(TEntity entity);
    protected abstract TEntity MapToEntity(TCreateDto dto);
    protected abstract void MapToEntity(TUpdateDto dto, TEntity entity);
}
