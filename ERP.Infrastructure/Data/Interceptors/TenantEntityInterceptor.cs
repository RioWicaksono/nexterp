using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Common.Modules;

namespace ERP.Infrastructure.Data.Interceptors;

/// <summary>
/// Interceptor that automatically sets OrganizationId on new ITenantEntity entities.
/// Resolves ICurrentUserService from the same scope as the DbContext (inherits HTTP request scope).
/// </summary>
public class TenantEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;
    private readonly IServiceProvider _serviceProvider;

    public TenantEntityInterceptor(ICurrentUserService currentUser, IServiceProvider serviceProvider)
    {
        _currentUser = currentUser;
        _serviceProvider = serviceProvider;
    }

    public TenantEntityInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _currentUser = null!;
    }

    private ICurrentUserService? GetCurrentUser()
    {
        if (_currentUser != null)
            return _currentUser;
        return _serviceProvider.GetService<ICurrentUserService>();
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context != null)
        {
            SetTenantInfo(eventData.Context);
        }
        return base.SavingChanges(eventData, result);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (eventData.Context != null)
        {
            SetTenantInfo(eventData.Context);
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
            SetTenantInfo(eventData.Context);
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
            SetTenantInfo(eventData.Context);
        }
        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private void SetTenantInfo(DbContext context)
    {
        var currentUser = GetCurrentUser();

        if (currentUser?.OrganizationId == null)
        {
            // Only log warning in development - this is expected for system operations
            // In production, system operations should have proper context
            return;
        }

        var tenantId = currentUser.OrganizationId.Value;

        // Only process Added entities to set tenant ID
        var entries = context.ChangeTracker.Entries<ITenantEntity>()
            .Where(e => e.State == EntityState.Added)
            .ToList();

        foreach (var entry in entries)
        {
            var tenantEntity = entry.Entity;

            // Only set if OrganizationId is Guid.Empty (not already set)
            if (tenantEntity.OrganizationId == Guid.Empty)
            {
                entry.Property(nameof(ITenantEntity.OrganizationId)).CurrentValue = tenantId;
            }
        }
    }
}
