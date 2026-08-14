using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using ERP.Application.Common.Interfaces;

namespace ERP.Infrastructure.Data.Interceptors;

/// <summary>
/// Interceptor that automatically populates CreatedAt, UpdatedAt, CreatedBy, UpdatedBy audit fields.
/// Resolves ICurrentUserService from the same scope as the DbContext (inherits HTTP request scope).
/// </summary>
public class AuditingInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;

    // Store the DbContext scope service provider for fallback
    private readonly IServiceProvider _serviceProvider;

    public AuditingInterceptor(ICurrentUserService currentUser, IServiceProvider serviceProvider)
    {
        _currentUser = currentUser;
        _serviceProvider = serviceProvider;
    }

    public AuditingInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        // Will resolve from service provider when first needed
        _currentUser = null!;
    }

    private ICurrentUserService GetCurrentUser()
    {
        // Try injected service first (preferred - shares HTTP request scope)
        if (_currentUser != null)
            return _currentUser;

        // Fallback: resolve from service provider
        // This handles cases where interceptor is created before HTTP context exists
        return _serviceProvider.GetRequiredService<ICurrentUserService>();
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context != null)
        {
            UpdateAuditing(eventData.Context);
        }
        return base.SavingChanges(eventData, result);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (eventData.Context != null)
        {
            UpdateAuditing(eventData.Context);
        }
        return base.SavedChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null)
        {
            UpdateAuditing(eventData.Context);
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null)
        {
            UpdateAuditing(eventData.Context);
        }
        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditing(DbContext context)
    {
        var currentUser = GetCurrentUser();
        var username = currentUser?.Username ?? "SYSTEM";
        var now = DateTime.UtcNow;

        // Filter entries by state to avoid unnecessary iterations
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added ||
                        e.State == EntityState.Modified ||
                        e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            if (entry.Entity is not Domain.Common.BaseEntity entity)
                continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    entity.CreatedAt = now;
                    entity.CreatedBy = username;
                    if (entity.UpdatedAt == null)
                    {
                        entity.UpdatedAt = now;
                        entity.UpdatedBy = username;
                    }
                    break;

                case EntityState.Modified:
                    entity.UpdatedAt = now;
                    entity.UpdatedBy = username;
                    break;

                case EntityState.Deleted:
                    if (!entity.IsDeleted)
                    {
                        entry.State = EntityState.Modified;
                        entity.MarkAsDeleted(username);
                        entity.UpdatedAt = now;
                        entity.UpdatedBy = username;
                    }
                    break;
            }
        }
    }
}
