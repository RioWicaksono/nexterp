using ERP.Application.Inventory.Commands.StockItems;
using ERP.Application.Inventory.Commands.Warehouses;
using ERP.Domain.Inventory.Enums;
using FluentValidation.TestHelper;
using Xunit;

namespace ERP.Application.UnitTests.Inventory;

/// <summary>
/// Unit tests for Inventory domain validators
/// </summary>
public class InventoryValidatorTests
{
    #region CreateStockItemCommandValidator Tests

    [Fact]
    public void CreateStockItemCommandValidator_ValidCommand_ShouldPass()
    {
        // Arrange
        var validator = new CreateStockItemCommandValidator();
        var command = new CreateStockItemCommand
        {
            Name = "Test Product",
            Code = "PRD001",
            Barcode = "1234567890",
            Description = "Test description",
            UnitOfMeasureId = Guid.NewGuid(),
            ReorderLevel = 10,
            MinimumStock = 5,
            MaximumStock = 100,
            StandardCost = 50.00m,
            StandardPrice = 75.00m,
            ValuationMethod = "AverageCost" // Case-sensitive enum value
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert - Note: ValuationMethod uses IsInEnum() which expects exact enum name
        // The actual enum is ValuationMethod enum with AverageCost value
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CreateStockItemCommand_Name_WhenEmptyOrNull_ShouldFail(string? name)
    {
        // Arrange
        var validator = new CreateStockItemCommandValidator();
        var command = new CreateStockItemCommand
        {
            Name = name!,
            UnitOfMeasureId = Guid.NewGuid()
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Item name is required");
    }

    [Fact]
    public void CreateStockItemCommand_Name_WhenExceeds200Characters_ShouldFail()
    {
        // Arrange
        var validator = new CreateStockItemCommandValidator();
        var command = new CreateStockItemCommand
        {
            Name = new string('A', 201),
            UnitOfMeasureId = Guid.NewGuid()
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Item name cannot exceed 200 characters");
    }

    [Fact]
    public void CreateStockItemCommand_Name_When200Characters_ShouldPass()
    {
        // Arrange
        var validator = new CreateStockItemCommandValidator();
        var command = new CreateStockItemCommand
        {
            Name = new string('A', 200),
            UnitOfMeasureId = Guid.NewGuid()
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateStockItemCommand_Code_WhenExceeds50Characters_ShouldFail()
    {
        // Arrange
        var validator = new CreateStockItemCommandValidator();
        var command = new CreateStockItemCommand
        {
            Name = "Test Product",
            Code = new string('A', 51),
            UnitOfMeasureId = Guid.NewGuid()
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Code cannot exceed 50 characters");
    }

    [Fact]
    public void CreateStockItemCommand_Barcode_WhenExceeds100Characters_ShouldFail()
    {
        // Arrange
        var validator = new CreateStockItemCommandValidator();
        var command = new CreateStockItemCommand
        {
            Name = "Test Product",
            Barcode = new string('1', 101),
            UnitOfMeasureId = Guid.NewGuid()
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Barcode)
            .WithErrorMessage("Barcode cannot exceed 100 characters");
    }

    [Fact]
    public void CreateStockItemCommand_UnitOfMeasureId_WhenEmpty_ShouldFail()
    {
        // Arrange
        var validator = new CreateStockItemCommandValidator();
        var command = new CreateStockItemCommand
        {
            Name = "Test Product",
            UnitOfMeasureId = Guid.Empty
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UnitOfMeasureId)
            .WithErrorMessage("Unit of measure is required");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(-0.01)]
    public void CreateStockItemCommand_StandardCost_WhenNegative_ShouldFail(decimal cost)
    {
        // Arrange
        var validator = new CreateStockItemCommandValidator();
        var command = new CreateStockItemCommand
        {
            Name = "Test Product",
            UnitOfMeasureId = Guid.NewGuid(),
            StandardCost = cost
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StandardCost)
            .WithErrorMessage("Standard cost cannot be negative");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(999999.99)]
    public void CreateStockItemCommand_StandardCost_WhenZeroOrPositive_ShouldPass(decimal cost)
    {
        // Arrange
        var validator = new CreateStockItemCommandValidator();
        var command = new CreateStockItemCommand
        {
            Name = "Test Product",
            UnitOfMeasureId = Guid.NewGuid(),
            StandardCost = cost
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.StandardCost);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(-0.01)]
    public void CreateStockItemCommand_StandardPrice_WhenNegative_ShouldFail(decimal price)
    {
        // Arrange
        var validator = new CreateStockItemCommandValidator();
        var command = new CreateStockItemCommand
        {
            Name = "Test Product",
            UnitOfMeasureId = Guid.NewGuid(),
            StandardPrice = price
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StandardPrice)
            .WithErrorMessage("Standard price cannot be negative");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(999999.99)]
    public void CreateStockItemCommand_StandardPrice_WhenZeroOrPositive_ShouldPass(decimal price)
    {
        // Arrange
        var validator = new CreateStockItemCommandValidator();
        var command = new CreateStockItemCommand
        {
            Name = "Test Product",
            UnitOfMeasureId = Guid.NewGuid(),
            StandardPrice = price
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.StandardPrice);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void CreateStockItemCommand_MinimumStock_WhenNegative_ShouldFail(decimal minStock)
    {
        // Arrange
        var validator = new CreateStockItemCommandValidator();
        var command = new CreateStockItemCommand
        {
            Name = "Test Product",
            UnitOfMeasureId = Guid.NewGuid(),
            MinimumStock = minStock
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MinimumStock)
            .WithErrorMessage("Minimum stock cannot be negative");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void CreateStockItemCommand_MaximumStock_WhenNegative_ShouldFail(decimal maxStock)
    {
        // Arrange - note: validator only checks negative when MinimumStock has value
        var validator = new CreateStockItemCommandValidator();
        var command = new CreateStockItemCommand
        {
            Name = "Test Product",
            UnitOfMeasureId = Guid.NewGuid(),
            MinimumStock = 0, // Set MinimumStock so the conditional validation runs
            MaximumStock = maxStock
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaximumStock)
            .WithErrorMessage("Maximum stock cannot be negative");
    }

    [Fact]
    public void CreateStockItemCommand_MaximumStock_WhenLessThanMinimumStock_ShouldFail()
    {
        // Arrange
        var validator = new CreateStockItemCommandValidator();
        var command = new CreateStockItemCommand
        {
            Name = "Test Product",
            UnitOfMeasureId = Guid.NewGuid(),
            MinimumStock = 100,
            MaximumStock = 50
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaximumStock)
            .WithErrorMessage("Maximum stock must be greater than minimum stock");
    }

    [Fact]
    public void CreateStockItemCommand_MaximumStock_WhenGreaterThanMinimumStock_ShouldPass()
    {
        // Arrange
        var validator = new CreateStockItemCommandValidator();
        var command = new CreateStockItemCommand
        {
            Name = "Test Product",
            UnitOfMeasureId = Guid.NewGuid(),
            MinimumStock = 50,
            MaximumStock = 100
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaximumStock);
    }

    [Theory]
    [InlineData("AverageCost")]
    [InlineData("FIFO")]
    [InlineData("LIFO")]
    [InlineData("StandardCost")]
    [InlineData("averagecost")]   // Case insensitive
    [InlineData("fifo")]           // Case insensitive
    public void CreateStockItemCommand_ValuationMethod_WhenValidValue_ShouldPass(string method)
    {
        // Arrange
        var validator = new CreateStockItemCommandValidator();
        var command = new CreateStockItemCommand
        {
            Name = "Test Product",
            UnitOfMeasureId = Guid.NewGuid(),
            ValuationMethod = method
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ValuationMethod);
    }

    [Theory]
    [InlineData("InvalidMethod")]
    [InlineData("WrongValue")]
    [InlineData("")]
    public void CreateStockItemCommand_ValuationMethod_WhenInvalidValue_ShouldFail(string method)
    {
        // Arrange
        var validator = new CreateStockItemCommandValidator();
        var command = new CreateStockItemCommand
        {
            Name = "Test Product",
            UnitOfMeasureId = Guid.NewGuid(),
            ValuationMethod = method
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ValuationMethod);
    }

    #endregion

    #region CreateWarehouseCommandValidator Tests

    [Fact]
    public void CreateWarehouseCommand_ValidCommand_ShouldPass()
    {
        // Arrange
        var validator = new CreateWarehouseCommandValidator();
        var command = new CreateWarehouseCommand
        {
            Name = "Main Warehouse",
            Code = "WH001",
            Description = "Primary storage facility",
            Address = "123 Industrial Ave",
            City = "Jakarta",
            Country = "Indonesia",
            Phone = "+62123456789",
            Email = "warehouse@company.com",
            IsDefault = true,
            AllowsNegativeStock = false
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CreateWarehouseCommand_Name_WhenEmptyOrNull_ShouldFail(string? name)
    {
        // Arrange
        var validator = new CreateWarehouseCommandValidator();
        var command = new CreateWarehouseCommand
        {
            Name = name!
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Warehouse name is required");
    }

    [Fact]
    public void CreateWarehouseCommand_Name_WhenExceeds200Characters_ShouldFail()
    {
        // Arrange
        var validator = new CreateWarehouseCommandValidator();
        var command = new CreateWarehouseCommand
        {
            Name = new string('A', 201)
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Warehouse name cannot exceed 200 characters");
    }

    [Fact]
    public void CreateWarehouseCommand_Name_When200Characters_ShouldPass()
    {
        // Arrange
        var validator = new CreateWarehouseCommandValidator();
        var command = new CreateWarehouseCommand
        {
            Name = new string('A', 200)
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateWarehouseCommand_Code_WhenExceeds50Characters_ShouldFail()
    {
        // Arrange
        var validator = new CreateWarehouseCommandValidator();
        var command = new CreateWarehouseCommand
        {
            Name = "Valid Name",
            Code = new string('A', 51)
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Code cannot exceed 50 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CreateWarehouseCommand_Email_WhenEmptyOrNull_ShouldPass(string? email)
    {
        // Arrange
        var validator = new CreateWarehouseCommandValidator();
        var command = new CreateWarehouseCommand
        {
            Name = "Main Warehouse",
            Email = email
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("plainaddress")]
    [InlineData("@nodomain.com")]
    public void CreateWarehouseCommand_Email_WhenInvalidFormat_ShouldFail(string email)
    {
        // Arrange
        var validator = new CreateWarehouseCommandValidator();
        var command = new CreateWarehouseCommand
        {
            Name = "Main Warehouse",
            Email = email
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Invalid email format");
    }

    [Theory]
    [InlineData("warehouse@company.com")]
    [InlineData("warehouse.company@sub.domain.com")]
    [InlineData("wh@co.id")]
    public void CreateWarehouseCommand_Email_WhenValidFormat_ShouldPass(string email)
    {
        // Arrange
        var validator = new CreateWarehouseCommandValidator();
        var command = new CreateWarehouseCommand
        {
            Name = "Main Warehouse",
            Email = email
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CreateWarehouseCommand_Phone_WhenEmptyOrNull_ShouldPass(string? phone)
    {
        // Arrange
        var validator = new CreateWarehouseCommandValidator();
        var command = new CreateWarehouseCommand
        {
            Name = "Main Warehouse",
            Phone = phone
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Phone);
    }

    [Theory]
    [InlineData("12345")]        // Too short (5 digits)
    [InlineData("12345678901234567890")]  // Too long (20 digits)
    [InlineData("abc1234567")]   // Contains letters
    [InlineData("1234-5678")]   // Contains hyphen
    [InlineData("(123)4567890")] // Contains parentheses
    public void CreateWarehouseCommand_Phone_WhenInvalidFormat_ShouldFail(string phone)
    {
        // Arrange
        var validator = new CreateWarehouseCommandValidator();
        var command = new CreateWarehouseCommand
        {
            Name = "Main Warehouse",
            Phone = phone
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Phone)
            .WithErrorMessage("Invalid phone number format");
    }

    [Theory]
    [InlineData("+62123456789")]     // International format
    [InlineData("0213456789")]       // Standard local
    [InlineData("+621234567890123")] // International with 15 digits
    [InlineData("1234567890")]       // 10 digits
    public void CreateWarehouseCommand_Phone_WhenValidFormat_ShouldPass(string phone)
    {
        // Arrange
        var validator = new CreateWarehouseCommandValidator();
        var command = new CreateWarehouseCommand
        {
            Name = "Main Warehouse",
            Phone = phone
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Phone);
    }

    #endregion
}
