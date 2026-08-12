using ERP.Application.Sales.Commands.Customers;
using ERP.Application.Sales.Commands.SalesOrders;
using ERP.Application.Sales.DTOs;
using FluentValidation.TestHelper;
using Xunit;

namespace ERP.Application.UnitTests.Sales;

/// <summary>
/// Unit tests for Sales domain validators
/// </summary>
public class SalesValidatorTests
{
    #region CreateCustomerCommandValidator Tests

    [Fact]
    public void CreateCustomerCommand_ValidCommand_ShouldPass()
    {
        // Arrange
        var validator = new CreateCustomerCommandValidator();
        var command = new CreateCustomerCommand
        {
            CustomerCode = "CUST001",
            CustomerName = "PT Maju Jaya",
            Type = "Company",
            TaxId = "01.234.567.8-001.000",
            Email = "contact@majujaya.com",
            Phone = "+6221123456",
            Mobile = "+6281234567890",
            BillingAddress = "Jl. Sudirman No. 123",
            BillingCity = "Jakarta",
            BillingCountry = "Indonesia",
            BillingPostalCode = "10220",
            CreditLimit = 50000000
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CreateCustomerCommand_CustomerCode_WhenEmptyOrNull_ShouldFail(string? code)
    {
        // Arrange
        var validator = new CreateCustomerCommandValidator();
        var command = new CreateCustomerCommand
        {
            CustomerCode = code!,
            CustomerName = "Valid Name",
            Type = "Individual"
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerCode)
            .WithErrorMessage("Customer code is required");
    }

    [Fact]
    public void CreateCustomerCommand_CustomerCode_WhenExceeds50Characters_ShouldFail()
    {
        // Arrange
        var validator = new CreateCustomerCommandValidator();
        var command = new CreateCustomerCommand
        {
            CustomerCode = new string('A', 51),
            CustomerName = "Valid Name",
            Type = "Individual"
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerCode)
            .WithErrorMessage("Customer code cannot exceed 50 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CreateCustomerCommand_CustomerName_WhenEmptyOrNull_ShouldFail(string? name)
    {
        // Arrange
        var validator = new CreateCustomerCommandValidator();
        var command = new CreateCustomerCommand
        {
            CustomerCode = "CUST001",
            CustomerName = name!,
            Type = "Individual"
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerName)
            .WithErrorMessage("Customer name is required");
    }

    [Fact]
    public void CreateCustomerCommand_CustomerName_WhenExceeds200Characters_ShouldFail()
    {
        // Arrange
        var validator = new CreateCustomerCommandValidator();
        var command = new CreateCustomerCommand
        {
            CustomerCode = "CUST001",
            CustomerName = new string('A', 201),
            Type = "Individual"
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerName)
            .WithErrorMessage("Customer name cannot exceed 200 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CreateCustomerCommand_Type_WhenEmptyOrNull_ShouldFail(string? type)
    {
        // Arrange
        var validator = new CreateCustomerCommandValidator();
        var command = new CreateCustomerCommand
        {
            CustomerCode = "CUST001",
            CustomerName = "Valid Name",
            Type = type!
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorMessage("Customer type is required");
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("Corporation")]
    [InlineData("Personal")]
    [InlineData("OTHER")]
    public void CreateCustomerCommand_Type_WhenInvalidValue_ShouldFail(string type)
    {
        // Arrange
        var validator = new CreateCustomerCommandValidator();
        var command = new CreateCustomerCommand
        {
            CustomerCode = "CUST001",
            CustomerName = "Valid Name",
            Type = type
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorMessage("Invalid customer type. Valid values: Individual, Company, Government");
    }

    [Theory]
    [InlineData("Individual")]
    [InlineData("Company")]
    [InlineData("Government")]
    [InlineData("INDIVIDUAL")]   // Case insensitive
    [InlineData("company")]       // Case insensitive
    public void CreateCustomerCommand_Type_WhenValidValue_ShouldPass(string type)
    {
        // Arrange
        var validator = new CreateCustomerCommandValidator();
        var command = new CreateCustomerCommand
        {
            CustomerCode = "CUST001",
            CustomerName = "Valid Name",
            Type = type
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Type);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CreateCustomerCommand_Email_WhenEmptyOrNull_ShouldPass(string? email)
    {
        // Arrange
        var validator = new CreateCustomerCommandValidator();
        var command = new CreateCustomerCommand
        {
            CustomerCode = "CUST001",
            CustomerName = "Valid Name",
            Type = "Individual",
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
    public void CreateCustomerCommand_Email_WhenInvalidFormat_ShouldFail(string email)
    {
        // Arrange
        var validator = new CreateCustomerCommandValidator();
        var command = new CreateCustomerCommand
        {
            CustomerCode = "CUST001",
            CustomerName = "Valid Name",
            Type = "Individual",
            Email = email
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Invalid email format");
    }

    [Theory]
    [InlineData("customer@company.com")]
    [InlineData("customer.company@sub.domain.com")]
    [InlineData("cust@co.id")]
    public void CreateCustomerCommand_Email_WhenValidFormat_ShouldPass(string email)
    {
        // Arrange
        var validator = new CreateCustomerCommandValidator();
        var command = new CreateCustomerCommand
        {
            CustomerCode = "CUST001",
            CustomerName = "Valid Name",
            Type = "Individual",
            Email = email
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("12345")]        // Too short (5 digits)
    [InlineData("12345678901234567890")]  // Too long (20 digits)
    [InlineData("abc1234567")]   // Contains letters
    public void CreateCustomerCommand_Phone_WhenInvalidFormat_ShouldFail(string phone)
    {
        // Arrange
        var validator = new CreateCustomerCommandValidator();
        var command = new CreateCustomerCommand
        {
            CustomerCode = "CUST001",
            CustomerName = "Valid Name",
            Type = "Individual",
            Phone = phone
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Phone)
            .WithErrorMessage("Invalid phone number format");
    }

    [Theory]
    [InlineData("+6221123456")]       // International format with area code (11 digits)
    [InlineData("02112345678")]        // Local with area code (11 digits)
    [InlineData("+6281234567890")]     // Mobile with country code (13 digits)
    [InlineData("1234567890")]          // 10 digits minimum
    public void CreateCustomerCommand_Phone_WhenValidFormat_ShouldPass(string phone)
    {
        // Arrange
        var validator = new CreateCustomerCommandValidator();
        var command = new CreateCustomerCommand
        {
            CustomerCode = "CUST001",
            CustomerName = "Valid Name",
            Type = "Individual",
            Phone = phone
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Phone);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-1000)]
    [InlineData(-50000.01)]
    public void CreateCustomerCommand_CreditLimit_WhenNegative_ShouldFail(decimal creditLimit)
    {
        // Arrange
        var validator = new CreateCustomerCommandValidator();
        var command = new CreateCustomerCommand
        {
            CustomerCode = "CUST001",
            CustomerName = "Valid Name",
            Type = "Individual",
            CreditLimit = creditLimit
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CreditLimit)
            .WithErrorMessage("Credit limit cannot be negative");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1000)]
    [InlineData(50000000)]
    [InlineData(999999999.99)]
    public void CreateCustomerCommand_CreditLimit_WhenZeroOrPositive_ShouldPass(decimal creditLimit)
    {
        // Arrange
        var validator = new CreateCustomerCommandValidator();
        var command = new CreateCustomerCommand
        {
            CustomerCode = "CUST001",
            CustomerName = "Valid Name",
            Type = "Individual",
            CreditLimit = creditLimit
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CreditLimit);
    }

    [Fact]
    public void CreateCustomerCommand_TaxId_WhenExceeds50Characters_ShouldFail()
    {
        // Arrange
        var validator = new CreateCustomerCommandValidator();
        var command = new CreateCustomerCommand
        {
            CustomerCode = "CUST001",
            CustomerName = "Valid Name",
            Type = "Company",
            TaxId = new string('A', 51)
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TaxId)
            .WithErrorMessage("Tax ID cannot exceed 50 characters");
    }

    #endregion

    #region CreateSalesOrderCommandValidator Tests

    [Fact]
    public void CreateSalesOrderCommand_ValidCommand_ShouldPass()
    {
        // Arrange
        var validator = new CreateSalesOrderCommandValidator();
        var command = new CreateSalesOrderCommand
        {
            OrderDate = DateTime.UtcNow,
            DeliveryDate = DateTime.UtcNow.AddDays(7),
            CustomerId = Guid.NewGuid(),
            PriceListId = Guid.NewGuid(),
            PaymentTermId = Guid.NewGuid(),
            Notes = "Urgent order",
            WarehouseId = Guid.NewGuid(),
            Lines = new List<CreateSalesOrderLineDto>
            {
                new CreateSalesOrderLineDto
                {
                    StockItemId = Guid.NewGuid(),
                    Description = "Product A",
                    Quantity = 10,
                    UnitPrice = 100,
                    UnitOfMeasureId = Guid.NewGuid(),
                    TaxRate = 10
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateSalesOrderCommand_CustomerId_WhenEmpty_ShouldFail()
    {
        // Arrange
        var validator = new CreateSalesOrderCommandValidator();
        var command = new CreateSalesOrderCommand
        {
            OrderDate = DateTime.UtcNow,
            CustomerId = Guid.Empty,
            Lines = new List<CreateSalesOrderLineDto>
            {
                new CreateSalesOrderLineDto
                {
                    StockItemId = Guid.NewGuid(),
                    Quantity = 10,
                    UnitPrice = 100,
                    UnitOfMeasureId = Guid.NewGuid()
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerId)
            .WithErrorMessage("Customer is required");
    }

    [Fact]
    public void CreateSalesOrderCommand_OrderDate_WhenDefault_ShouldFail()
    {
        // Arrange
        var validator = new CreateSalesOrderCommandValidator();
        var command = new CreateSalesOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            OrderDate = default,
            Lines = new List<CreateSalesOrderLineDto>
            {
                new CreateSalesOrderLineDto
                {
                    StockItemId = Guid.NewGuid(),
                    Quantity = 10,
                    UnitPrice = 100,
                    UnitOfMeasureId = Guid.NewGuid()
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OrderDate)
            .WithErrorMessage("Order date is required");
    }

    [Fact]
    public void CreateSalesOrderCommand_OrderDate_WhenTooFarInFuture_ShouldFail()
    {
        // Arrange
        var validator = new CreateSalesOrderCommandValidator();
        var command = new CreateSalesOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            OrderDate = DateTime.UtcNow.AddDays(5),
            Lines = new List<CreateSalesOrderLineDto>
            {
                new CreateSalesOrderLineDto
                {
                    StockItemId = Guid.NewGuid(),
                    Quantity = 10,
                    UnitPrice = 100,
                    UnitOfMeasureId = Guid.NewGuid()
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OrderDate)
            .WithErrorMessage("Order date cannot be in the future");
    }

    [Fact]
    public void CreateSalesOrderCommand_Lines_WhenEmpty_ShouldFail()
    {
        // Arrange
        var validator = new CreateSalesOrderCommandValidator();
        var command = new CreateSalesOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            OrderDate = DateTime.UtcNow,
            Lines = new List<CreateSalesOrderLineDto>()
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Lines)
            .WithErrorMessage("Sales order must have at least one line");
    }

    [Fact]
    public void CreateSalesOrderCommand_LineQuantity_WhenNegative_ShouldFail()
    {
        // Arrange
        var validator = new CreateSalesOrderCommandValidator();
        var command = new CreateSalesOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            OrderDate = DateTime.UtcNow,
            Lines = new List<CreateSalesOrderLineDto>
            {
                new CreateSalesOrderLineDto
                {
                    StockItemId = Guid.NewGuid(),
                    Quantity = -5,
                    UnitPrice = 100,
                    UnitOfMeasureId = Guid.NewGuid()
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert - Must() on collection reports error on parent property "Lines"
        result.ShouldHaveValidationErrorFor(x => x.Lines)
            .WithErrorMessage("All line quantities must be greater than zero");
    }

    [Fact]
    public void CreateSalesOrderCommand_LineQuantity_WhenZero_ShouldFail()
    {
        // Arrange
        var validator = new CreateSalesOrderCommandValidator();
        var command = new CreateSalesOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            OrderDate = DateTime.UtcNow,
            Lines = new List<CreateSalesOrderLineDto>
            {
                new CreateSalesOrderLineDto
                {
                    StockItemId = Guid.NewGuid(),
                    Quantity = 0,
                    UnitPrice = 100,
                    UnitOfMeasureId = Guid.NewGuid()
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert - Must() on collection reports error on parent property "Lines"
        result.ShouldHaveValidationErrorFor(x => x.Lines)
            .WithErrorMessage("All line quantities must be greater than zero");
    }

    [Fact]
    public void CreateSalesOrderCommand_LineStockItemId_WhenEmpty_ShouldFail()
    {
        // Arrange
        var validator = new CreateSalesOrderCommandValidator();
        var command = new CreateSalesOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            OrderDate = DateTime.UtcNow,
            Lines = new List<CreateSalesOrderLineDto>
            {
                new CreateSalesOrderLineDto
                {
                    StockItemId = Guid.Empty,
                    Quantity = 10,
                    UnitPrice = 100,
                    UnitOfMeasureId = Guid.NewGuid()
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Lines[0].StockItemId")
            .WithErrorMessage("Stock item is required for each line");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(-0.01)]
    public void CreateSalesOrderCommand_LineUnitPrice_WhenNegative_ShouldFail(decimal unitPrice)
    {
        // Arrange
        var validator = new CreateSalesOrderCommandValidator();
        var command = new CreateSalesOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            OrderDate = DateTime.UtcNow,
            Lines = new List<CreateSalesOrderLineDto>
            {
                new CreateSalesOrderLineDto
                {
                    StockItemId = Guid.NewGuid(),
                    Quantity = 10,
                    UnitPrice = unitPrice,
                    UnitOfMeasureId = Guid.NewGuid()
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Lines[0].UnitPrice")
            .WithErrorMessage("Unit price cannot be negative");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(999999.99)]
    public void CreateSalesOrderCommand_LineUnitPrice_WhenZeroOrPositive_ShouldPass(decimal unitPrice)
    {
        // Arrange
        var validator = new CreateSalesOrderCommandValidator();
        var command = new CreateSalesOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            OrderDate = DateTime.UtcNow,
            Lines = new List<CreateSalesOrderLineDto>
            {
                new CreateSalesOrderLineDto
                {
                    StockItemId = Guid.NewGuid(),
                    Quantity = 10,
                    UnitPrice = unitPrice,
                    UnitOfMeasureId = Guid.NewGuid()
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor("Lines[0].UnitPrice");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void CreateSalesOrderCommand_LineTaxRate_WhenNegative_ShouldFail(decimal taxRate)
    {
        // Arrange
        var validator = new CreateSalesOrderCommandValidator();
        var command = new CreateSalesOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            OrderDate = DateTime.UtcNow,
            Lines = new List<CreateSalesOrderLineDto>
            {
                new CreateSalesOrderLineDto
                {
                    StockItemId = Guid.NewGuid(),
                    Quantity = 10,
                    UnitPrice = 100,
                    UnitOfMeasureId = Guid.NewGuid(),
                    TaxRate = taxRate
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Lines[0].TaxRate")
            .WithErrorMessage("Tax rate cannot be negative");
    }

    [Theory]
    [InlineData(101)]
    [InlineData(150)]
    [InlineData(1000)]
    public void CreateSalesOrderCommand_LineTaxRate_WhenExceeds100_ShouldFail(decimal taxRate)
    {
        // Arrange
        var validator = new CreateSalesOrderCommandValidator();
        var command = new CreateSalesOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            OrderDate = DateTime.UtcNow,
            Lines = new List<CreateSalesOrderLineDto>
            {
                new CreateSalesOrderLineDto
                {
                    StockItemId = Guid.NewGuid(),
                    Quantity = 10,
                    UnitPrice = 100,
                    UnitOfMeasureId = Guid.NewGuid(),
                    TaxRate = taxRate
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Lines[0].TaxRate")
            .WithErrorMessage("Tax rate cannot exceed 100%");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(100)]
    public void CreateSalesOrderCommand_LineTaxRate_WhenValidRange_ShouldPass(decimal taxRate)
    {
        // Arrange
        var validator = new CreateSalesOrderCommandValidator();
        var command = new CreateSalesOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            OrderDate = DateTime.UtcNow,
            Lines = new List<CreateSalesOrderLineDto>
            {
                new CreateSalesOrderLineDto
                {
                    StockItemId = Guid.NewGuid(),
                    Quantity = 10,
                    UnitPrice = 100,
                    UnitOfMeasureId = Guid.NewGuid(),
                    TaxRate = taxRate
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor("Lines[0].TaxRate");
    }

    [Fact]
    public void CreateSalesOrderCommand_MultipleLines_AllValid_ShouldPass()
    {
        // Arrange
        var validator = new CreateSalesOrderCommandValidator();
        var command = new CreateSalesOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            OrderDate = DateTime.UtcNow,
            Lines = new List<CreateSalesOrderLineDto>
            {
                new CreateSalesOrderLineDto
                {
                    StockItemId = Guid.NewGuid(),
                    Quantity = 10,
                    UnitPrice = 100,
                    UnitOfMeasureId = Guid.NewGuid(),
                    TaxRate = 10
                },
                new CreateSalesOrderLineDto
                {
                    StockItemId = Guid.NewGuid(),
                    Quantity = 5,
                    UnitPrice = 200,
                    UnitOfMeasureId = Guid.NewGuid(),
                    TaxRate = 11
                },
                new CreateSalesOrderLineDto
                {
                    StockItemId = Guid.NewGuid(),
                    Quantity = 1,
                    UnitPrice = 500,
                    UnitOfMeasureId = Guid.NewGuid(),
                    TaxRate = 0
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}
