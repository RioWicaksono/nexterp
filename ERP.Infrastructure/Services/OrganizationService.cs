using ERP.Application.Common.Interfaces;

namespace ERP.Infrastructure.Services;

/// <summary>
/// Organization service implementation.
/// </summary>
public class OrganizationService : IOrganizationService
{
    private readonly IApplicationDbContext _context;
    private readonly Dictionary<Guid, string> _nameCache = new();
    private DateTime _cacheExpiry = DateTime.MinValue;

    public OrganizationService(IApplicationDbContext context)
    {
        _context = context;
    }

    public string GetName(Guid organizationId)
    {
        if (_cacheExpiry < DateTime.UtcNow)
        {
            RefreshCache();
        }

        return _nameCache.GetValueOrDefault(organizationId, "Unknown");
    }

    public async Task<bool> ExistsAsync(Guid organizationId)
    {
        return await Task.FromResult(
            _context.Organizations.Any(o => o.Id == organizationId));
    }

    private void RefreshCache()
    {
        _nameCache.Clear();
        var orgs = _context.Organizations.ToList();
        foreach (var org in orgs)
        {
            _nameCache[org.Id] = org.Name;
        }
        _cacheExpiry = DateTime.UtcNow.AddMinutes(5);
    }
}
