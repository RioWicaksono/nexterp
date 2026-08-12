using ERP.Domain.Common;
using ERP.Domain.Inventory.Entities;

namespace ERP.Domain.Base;

/// <summary>
/// Represents an organization/company in the ERP system.
/// This serves as the tenant boundary for multi-tenancy.
/// </summary>
public class Organization : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; }
    public string? TaxId { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? Country { get; private set; }
    public string? PostalCode { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? LicenseExpiry { get; private set; }

    // Navigation properties
    private readonly List<User> _users = new();
    public IReadOnlyCollection<User> Users => _users.AsReadOnly();

    private readonly List<Warehouse> _warehouses = new();
    public IReadOnlyCollection<Warehouse> Warehouses => _warehouses.AsReadOnly();

    // Factory method
    public static Organization Create(
        string name,
        string? code = null,
        string? taxId = null,
        string? phone = null,
        string? email = null,
        string? address = null,
        string? city = null,
        string? country = null,
        string? postalCode = null,
        DateTime? licenseExpiry = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Organization name is required", nameof(name));

        return new Organization
        {
            Name = name.Trim(),
            Code = code?.Trim().ToUpperInvariant(),
            TaxId = taxId?.Trim(),
            Phone = phone?.Trim(),
            Email = email?.Trim().ToLowerInvariant(),
            Address = address?.Trim(),
            City = city?.Trim(),
            Country = country?.Trim(),
            PostalCode = postalCode?.Trim(),
            LicenseExpiry = licenseExpiry
        };
    }

    public void Update(
        string? name = null,
        string? code = null,
        string? taxId = null,
        string? phone = null,
        string? email = null,
        string? address = null,
        string? city = null,
        string? country = null,
        string? postalCode = null,
        DateTime? licenseExpiry = null)
    {
        Name = name?.Trim() ?? Name;
        Code = code?.Trim().ToUpperInvariant() ?? Code;
        TaxId = taxId?.Trim() ?? TaxId;
        Phone = phone?.Trim() ?? Phone;
        Email = email?.Trim().ToLowerInvariant() ?? Email;
        Address = address?.Trim() ?? Address;
        City = city?.Trim() ?? City;
        Country = country?.Trim() ?? Country;
        PostalCode = postalCode?.Trim() ?? PostalCode;
        LicenseExpiry = licenseExpiry ?? LicenseExpiry;
        UpdateTimestamp();
    }

    public void Activate() { IsActive = true; UpdateTimestamp(); }
    public void Deactivate() { IsActive = false; UpdateTimestamp(); }

    public bool IsLicenseValid() =>
        !LicenseExpiry.HasValue || LicenseExpiry.Value > DateTime.UtcNow;
}
