using Xunit;
using ERP.Domain.Base;

namespace ERP.Domain.UnitTests;

/// <summary>
/// Unit tests for Organization entity
/// </summary>
public class OrganizationTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateOrganization()
    {
        // Arrange
        var name = "Test Company";
        var code = "TC001";
        var taxId = "12.345.678.9-001.000";
        var email = "contact@testcompany.com";

        // Act
        var org = Organization.Create(name, code, taxId, email: email);

        // Assert
        Assert.NotEqual(Guid.Empty, org.Id);
        Assert.Equal(name, org.Name);
        Assert.Equal(code, org.Code);
        Assert.Equal(taxId, org.TaxId);
        Assert.Equal(email.ToLowerInvariant(), org.Email);
        Assert.True(org.IsActive);
        Assert.False(org.IsDeleted);
    }

    [Fact]
    public void Create_WithValidData_ShouldTrimAndUppercaseCode()
    {
        // Arrange
        var name = "Test Company";
        var code = "  tc001  ";

        // Act
        var org = Organization.Create(name, code);

        // Assert
        Assert.Equal("TC001", org.Code);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Organization.Create(""));
        Assert.Contains("Organization name is required", exception.Message);
    }

    [Fact]
    public void Create_WithWhitespaceName_ShouldThrowException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Organization.Create("   "));
        Assert.Contains("Organization name is required", exception.Message);
    }

    [Fact]
    public void Update_ShouldUpdateFields()
    {
        // Arrange
        var org = CreateTestOrganization();
        var newName = "Updated Company";
        var newCity = "Jakarta";
        var newCountry = "Indonesia";

        // Act
        org.Update(city: newCity, country: newCountry);
        org.Update(name: newName);

        // Assert
        Assert.Equal(newName, org.Name);
        Assert.Equal(newCity, org.City);
        Assert.Equal(newCountry, org.Country);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var org = CreateTestOrganization();
        org.Deactivate();

        // Act
        org.Activate();

        // Assert
        Assert.True(org.IsActive);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var org = CreateTestOrganization();

        // Act
        org.Deactivate();

        // Assert
        Assert.False(org.IsActive);
    }

    [Fact]
    public void IsLicenseValid_WithNoExpiry_ShouldReturnTrue()
    {
        // Arrange
        var org = CreateTestOrganization();

        // Act
        var isValid = org.IsLicenseValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsLicenseValid_WithFutureExpiry_ShouldReturnTrue()
    {
        // Arrange
        var org = Organization.Create(
            "Test Company",
            licenseExpiry: DateTime.UtcNow.AddDays(30));

        // Act
        var isValid = org.IsLicenseValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsLicenseValid_WithPastExpiry_ShouldReturnFalse()
    {
        // Arrange
        var org = Organization.Create(
            "Test Company",
            licenseExpiry: DateTime.UtcNow.AddDays(-1));

        // Act
        var isValid = org.IsLicenseValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void MarkAsDeleted_ShouldSetIsDeletedToTrue()
    {
        // Arrange
        var org = CreateTestOrganization();

        // Act
        org.MarkAsDeleted();

        // Assert
        Assert.True(org.IsDeleted);
    }

    private static Organization CreateTestOrganization()
    {
        return Organization.Create("Test Company", "TC001");
    }
}
