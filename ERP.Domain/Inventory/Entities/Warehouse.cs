using ERP.Domain.Common;

namespace ERP.Domain.Inventory.Entities;

/// <summary>
/// Warehouse entity representing a storage location
/// </summary>
public class Warehouse : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; }
    public string? Description { get; private set; }
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? Country { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsDefault { get; private set; }
    public bool AllowsNegativeStock { get; private set; } = true;

    // Navigation properties
    private readonly List<StockItem> _stockItems = new();
    public IReadOnlyCollection<StockItem> StockItems => _stockItems.AsReadOnly();

    private readonly List<StockTransaction> _transactions = new();
    public IReadOnlyCollection<StockTransaction> Transactions => _transactions.AsReadOnly();

    // Factory method
    public static Warehouse Create(
        Guid organizationId,
        string name,
        string? code = null,
        string? description = null,
        string? address = null,
        string? city = null,
        string? country = null,
        string? phone = null,
        string? email = null,
        bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Warehouse name is required", nameof(name));

        return new Warehouse
        {
            OrganizationId = organizationId,
            Name = name.Trim(),
            Code = code?.Trim().ToUpperInvariant(),
            Description = description?.Trim(),
            Address = address?.Trim(),
            City = city?.Trim(),
            Country = country?.Trim(),
            Phone = phone?.Trim(),
            Email = email?.Trim().ToLowerInvariant(),
            IsDefault = isDefault
        };
    }

    public void Update(
        string? name = null,
        string? code = null,
        string? description = null,
        string? address = null,
        string? city = null,
        string? country = null,
        string? phone = null,
        string? email = null)
    {
        Name = name?.Trim() ?? Name;
        Code = code?.Trim().ToUpperInvariant() ?? Code;
        Description = description?.Trim() ?? Description;
        Address = address?.Trim() ?? Address;
        City = city?.Trim() ?? City;
        Country = country?.Trim() ?? Country;
        Phone = phone?.Trim() ?? Phone;
        Email = email?.Trim().ToLowerInvariant() ?? Email;
        UpdateTimestamp();
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        UpdateTimestamp();
    }

    public void RemoveDefault()
    {
        IsDefault = false;
        UpdateTimestamp();
    }

    public void Activate() { IsActive = true; UpdateTimestamp(); }
    public void Deactivate() { IsActive = false; UpdateTimestamp(); }

    public void AllowNegativeStock() { AllowsNegativeStock = true; UpdateTimestamp(); }
    public void DisallowNegativeStock() { AllowsNegativeStock = false; UpdateTimestamp(); }
}
