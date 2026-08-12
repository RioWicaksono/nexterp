using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace ERP.Application.Common.Interfaces;

/// <summary>
/// Generic repository interface
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository with includes support
/// </summary>
public interface IRepositoryWithIncludes<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, bool includeRelated = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(bool includeRelated = false, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, bool includeRelated = false, CancellationToken cancellationToken = default);
}
