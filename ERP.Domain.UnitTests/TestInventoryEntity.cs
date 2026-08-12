using Xunit;
using ERP.Domain.Common;
using ERP.Domain.Inventory.Entities;
using ERP.Domain.Inventory.Enums;

namespace ERP.Domain.UnitTests;

/// <summary>
/// Unit tests for Inventory domain entities
/// </summary>
public class TestInventoryEntity
{
    #region StockItem Tests

    [Fact]
    public void StockItem_Create_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var uomId = Guid.NewGuid();
        var name = "Test Product";
        var code = "PRD001";

        // Act
        var stockItem = StockItem.Create(
            organizationId: orgId,
            name: name,
            unitOfMeasureId: uomId,
            code: code,
            standardCost: 100m,
            standardPrice: 150m);

        // Assert
        Assert.NotNull(stockItem);
        Assert.Equal(orgId, stockItem.OrganizationId);
        Assert.Equal(name, stockItem.Name);
        Assert.Equal(code, stockItem.Code);
        Assert.Equal(uomId, stockItem.UnitOfMeasureId);
        Assert.Equal(100m, stockItem.StandardCost);
        Assert.Equal(150m, stockItem.StandardPrice);
        Assert.Equal(ValuationMethod.AverageCost, stockItem.ValuationMethod);
        Assert.True(stockItem.IsActive);
        Assert.Equal(0, stockItem.TotalStock);
    }

    [Fact]
    public void StockItem_Create_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var uomId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            StockItem.Create(
                organizationId: orgId,
                name: "",
                unitOfMeasureId: uomId));

        Assert.Contains("Item name is required", exception.Message);
    }

    [Fact]
    public void StockItem_Create_WithWhitespaceName_ShouldThrowArgumentException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var uomId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            StockItem.Create(
                organizationId: orgId,
                name: "   ",
                unitOfMeasureId: uomId));

        Assert.Contains("Item name is required", exception.Message);
    }

    [Fact]
    public void StockItem_Create_ShouldTrimAndUppercaseCode()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var uomId = Guid.NewGuid();
        var code = "prd001";

        // Act
        var stockItem = StockItem.Create(
            organizationId: orgId,
            name: "Test",
            unitOfMeasureId: uomId,
            code: code);

        // Assert
        Assert.Equal("PRD001", stockItem.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void StockItemWarehouse_AddStock_WithInvalidQuantity_ShouldThrowArgumentException(decimal invalidQuantity)
    {
        // Arrange
        var stockItemWarehouse = StockItemWarehouse.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            stockItemWarehouse.AddStock(invalidQuantity, 10m));

        Assert.Contains("Quantity must be positive", exception.Message);
    }

    [Fact]
    public void StockItemWarehouse_AddStock_WithValidQuantity_ShouldIncreaseStock()
    {
        // Arrange
        var stockItemWarehouse = StockItemWarehouse.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act
        stockItemWarehouse.AddStock(100m, 25m);

        // Assert
        Assert.Equal(100m, stockItemWarehouse.Quantity);
        Assert.Equal(25m, stockItemWarehouse.AverageCost);
        Assert.NotNull(stockItemWarehouse.LastStockIn);
    }

    [Fact]
    public void StockItemWarehouse_RemoveStock_WithExceedingQuantity_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var stockItemWarehouse = StockItemWarehouse.Create(Guid.NewGuid(), Guid.NewGuid());
        stockItemWarehouse.SetInitialStock(50m, 10m);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            stockItemWarehouse.RemoveStock(100m));

        Assert.Contains("Insufficient stock", exception.Message);
    }

    [Fact]
    public void StockItemWarehouse_Reserve_WithInsufficientAvailableStock_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var stockItemWarehouse = StockItemWarehouse.Create(Guid.NewGuid(), Guid.NewGuid());
        stockItemWarehouse.SetInitialStock(50m, 10m);
        stockItemWarehouse.Reserve(30m);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            stockItemWarehouse.Reserve(25m));

        Assert.Contains("Insufficient available stock", exception.Message);
    }

    [Fact]
    public void StockItemWarehouse_CalculateAvailableQuantity_ShouldExcludeReserved()
    {
        // Arrange
        var stockItemWarehouse = StockItemWarehouse.Create(Guid.NewGuid(), Guid.NewGuid());
        stockItemWarehouse.SetInitialStock(100m, 10m);

        // Act
        stockItemWarehouse.Reserve(25m);

        // Assert
        Assert.Equal(100m, stockItemWarehouse.Quantity);
        Assert.Equal(25m, stockItemWarehouse.ReservedQuantity);
        Assert.Equal(75m, stockItemWarehouse.AvailableQuantity);
    }

    [Theory]
    [InlineData(ValuationMethod.AverageCost)]
    [InlineData(ValuationMethod.FIFO)]
    [InlineData(ValuationMethod.LIFO)]
    [InlineData(ValuationMethod.StandardCost)]
    public void StockItem_SetValuationMethod_ShouldUpdateValuationMethod(ValuationMethod method)
    {
        // Arrange
        var stockItem = StockItem.Create(
            organizationId: Guid.NewGuid(),
            name: "Test Product",
            unitOfMeasureId: Guid.NewGuid());

        // Act
        stockItem.SetValuationMethod(method);

        // Assert
        Assert.Equal(method, stockItem.ValuationMethod);
    }

    [Fact]
    public void StockItem_AddWarehouse_ShouldPreventDuplicateWarehouse()
    {
        // Arrange
        var stockItem = StockItem.Create(
            organizationId: Guid.NewGuid(),
            name: "Test Product",
            unitOfMeasureId: Guid.NewGuid());
        var warehouseId = Guid.NewGuid();
        var itemWarehouse = StockItemWarehouse.Create(stockItem.Id, warehouseId);

        // Act
        stockItem.AddWarehouse(itemWarehouse);
        stockItem.AddWarehouse(itemWarehouse);

        // Assert
        Assert.Single(stockItem.Warehouses);
    }

    [Fact]
    public void StockItem_CalculateBelowReorderLevel_ShouldReturnTrueWhenBelow()
    {
        // Arrange
        var stockItem = StockItem.Create(
            organizationId: Guid.NewGuid(),
            name: "Test Product",
            unitOfMeasureId: Guid.NewGuid(),
            reorderLevel: 50);

        var itemWarehouse = StockItemWarehouse.Create(stockItem.Id, Guid.NewGuid());
        itemWarehouse.SetInitialStock(30m, 10m);
        stockItem.AddWarehouse(itemWarehouse);

        // Act & Assert
        Assert.True(stockItem.IsBelowReorderLevel);
    }

    #endregion

    #region Warehouse Tests

    [Fact]
    public void Warehouse_Create_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var name = "Main Warehouse";
        var code = "WH001";
        var email = "warehouse@test.com";

        // Act
        var warehouse = Warehouse.Create(
            organizationId: orgId,
            name: name,
            code: code,
            email: email,
            isDefault: true);

        // Assert
        Assert.NotNull(warehouse);
        Assert.Equal(orgId, warehouse.OrganizationId);
        Assert.Equal(name, warehouse.Name);
        Assert.Equal(code, warehouse.Code);
        Assert.Equal(email.ToLowerInvariant(), warehouse.Email);
        Assert.True(warehouse.IsDefault);
        Assert.True(warehouse.IsActive);
        Assert.True(warehouse.AllowsNegativeStock);
    }

    [Fact]
    public void Warehouse_Create_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var orgId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Warehouse.Create(
                organizationId: orgId,
                name: ""));

        Assert.Contains("Warehouse name is required", exception.Message);
    }

    [Fact]
    public void Warehouse_Create_ShouldTrimAndUppercaseCode()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var code = "wh001";

        // Act
        var warehouse = Warehouse.Create(
            organizationId: orgId,
            name: "Test",
            code: code);

        // Assert
        Assert.Equal("WH001", warehouse.Code);
    }

    [Fact]
    public void Warehouse_SetAsDefault_ShouldUpdateDefaultFlag()
    {
        // Arrange
        var warehouse = Warehouse.Create(
            organizationId: Guid.NewGuid(),
            name: "Test Warehouse",
            isDefault: false);

        // Act
        warehouse.SetAsDefault();

        // Assert
        Assert.True(warehouse.IsDefault);
    }

    [Fact]
    public void Warehouse_DisallowNegativeStock_ShouldUpdateFlag()
    {
        // Arrange
        var warehouse = Warehouse.Create(
            organizationId: Guid.NewGuid(),
            name: "Test Warehouse",
            isDefault: false);

        // Act
        warehouse.DisallowNegativeStock();

        // Assert
        Assert.False(warehouse.AllowsNegativeStock);
    }

    [Fact]
    public void Warehouse_Update_ShouldUpdateAllFields()
    {
        // Arrange
        var warehouse = Warehouse.Create(
            organizationId: Guid.NewGuid(),
            name: "Original Name",
            code: "WH001");

        // Act
        warehouse.Update(
            name: "Updated Name",
            code: "WH002",
            city: "Jakarta",
            country: "Indonesia");

        // Assert
        Assert.Equal("Updated Name", warehouse.Name);
        Assert.Equal("WH002", warehouse.Code);
        Assert.Equal("Jakarta", warehouse.City);
        Assert.Equal("Indonesia", warehouse.Country);
    }

    #endregion

    #region StockTransaction Tests

    [Fact]
    public void StockTransaction_Create_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var stockItemId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var transactionNumber = "ST001";
        var transactionDate = DateTime.UtcNow;
        var quantity = 100m;
        var unitCost = 25m;

        // Act
        var transaction = StockTransaction.Create(
            organizationId: orgId,
            transactionNumber: transactionNumber,
            type: StockTransactionType.PurchaseReceipt,
            transactionDate: transactionDate,
            stockItemId: stockItemId,
            warehouseId: warehouseId,
            quantity: quantity,
            unitCost: unitCost);

        // Assert
        Assert.NotNull(transaction);
        Assert.Equal(orgId, transaction.OrganizationId);
        Assert.Equal(transactionNumber, transaction.TransactionNumber);
        Assert.Equal(StockTransactionType.PurchaseReceipt, transaction.Type);
        Assert.Equal(quantity, transaction.Quantity);
        Assert.Equal(unitCost, transaction.UnitCost);
        Assert.Equal(Math.Round(quantity * unitCost, 2), transaction.TotalAmount);
        Assert.Equal(StockTransactionStatus.Pending, transaction.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-50.5)]
    public void StockTransaction_Create_WithZeroOrNegativeQuantity_ShouldThrowArgumentException(decimal invalidQuantity)
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            StockTransaction.Create(
                organizationId: Guid.NewGuid(),
                transactionNumber: "ST001",
                type: StockTransactionType.PurchaseReceipt,
                transactionDate: DateTime.UtcNow,
                stockItemId: Guid.NewGuid(),
                warehouseId: Guid.NewGuid(),
                quantity: invalidQuantity,
                unitCost: 10m));

        Assert.Contains("Quantity must be positive", exception.Message);
    }

    [Fact]
    public void StockTransaction_Create_WithEmptyTransactionNumber_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            StockTransaction.Create(
                organizationId: Guid.NewGuid(),
                transactionNumber: "",
                type: StockTransactionType.PurchaseReceipt,
                transactionDate: DateTime.UtcNow,
                stockItemId: Guid.NewGuid(),
                warehouseId: Guid.NewGuid(),
                quantity: 100m,
                unitCost: 10m));

        Assert.Contains("Transaction number is required", exception.Message);
    }

    [Fact]
    public void StockTransaction_Approve_WhenPending_ShouldChangeStatusToApproved()
    {
        // Arrange
        var transaction = StockTransaction.Create(
            organizationId: Guid.NewGuid(),
            transactionNumber: "ST001",
            type: StockTransactionType.PurchaseReceipt,
            transactionDate: DateTime.UtcNow,
            stockItemId: Guid.NewGuid(),
            warehouseId: Guid.NewGuid(),
            quantity: 100m,
            unitCost: 10m);

        // Act
        transaction.Approve();

        // Assert
        Assert.Equal(StockTransactionStatus.Approved, transaction.Status);
    }

    [Fact]
    public void StockTransaction_Approve_WhenNotPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var transaction = StockTransaction.Create(
            organizationId: Guid.NewGuid(),
            transactionNumber: "ST001",
            type: StockTransactionType.PurchaseReceipt,
            transactionDate: DateTime.UtcNow,
            stockItemId: Guid.NewGuid(),
            warehouseId: Guid.NewGuid(),
            quantity: 100m,
            unitCost: 10m);
        transaction.Approve();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => transaction.Approve());
        Assert.Contains("Only pending transactions can be approved", exception.Message);
    }

    [Fact]
    public void StockTransaction_Complete_WhenApproved_ShouldChangeStatusToCompleted()
    {
        // Arrange
        var transaction = StockTransaction.Create(
            organizationId: Guid.NewGuid(),
            transactionNumber: "ST001",
            type: StockTransactionType.PurchaseReceipt,
            transactionDate: DateTime.UtcNow,
            stockItemId: Guid.NewGuid(),
            warehouseId: Guid.NewGuid(),
            quantity: 100m,
            unitCost: 10m);
        transaction.Approve();

        // Act
        transaction.Complete();

        // Assert
        Assert.Equal(StockTransactionStatus.Completed, transaction.Status);
    }

    [Fact]
    public void StockTransaction_Reject_ShouldAddRejectionReason()
    {
        // Arrange
        var transaction = StockTransaction.Create(
            organizationId: Guid.NewGuid(),
            transactionNumber: "ST001",
            type: StockTransactionType.PurchaseReceipt,
            transactionDate: DateTime.UtcNow,
            stockItemId: Guid.NewGuid(),
            warehouseId: Guid.NewGuid(),
            quantity: 100m,
            unitCost: 10m);

        // Act
        transaction.Reject("Invalid quantity");

        // Assert
        Assert.Equal(StockTransactionStatus.Rejected, transaction.Status);
        Assert.Contains("Invalid quantity", transaction.Notes);
    }

    [Fact]
    public void StockTransaction_Cancel_WhenCompleted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var transaction = StockTransaction.Create(
            organizationId: Guid.NewGuid(),
            transactionNumber: "ST001",
            type: StockTransactionType.PurchaseReceipt,
            transactionDate: DateTime.UtcNow,
            stockItemId: Guid.NewGuid(),
            warehouseId: Guid.NewGuid(),
            quantity: 100m,
            unitCost: 10m);
        transaction.Approve();
        transaction.Complete();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => transaction.Cancel());
        Assert.Contains("Cannot cancel completed transaction", exception.Message);
    }

    [Theory]
    [InlineData(StockTransactionType.PurchaseReceipt)]
    [InlineData(StockTransactionType.SalesDelivery)]
    [InlineData(StockTransactionType.StockTransfer)]
    [InlineData(StockTransactionType.StockAdjustment)]
    public void StockTransaction_Create_WithDifferentTypes_ShouldCreateSuccessfully(StockTransactionType type)
    {
        // Arrange & Act
        var transaction = StockTransaction.Create(
            organizationId: Guid.NewGuid(),
            transactionNumber: "ST001",
            type: type,
            transactionDate: DateTime.UtcNow,
            stockItemId: Guid.NewGuid(),
            warehouseId: Guid.NewGuid(),
            quantity: 100m,
            unitCost: 10m);

        // Assert
        Assert.Equal(type, transaction.Type);
    }

    #endregion
}
