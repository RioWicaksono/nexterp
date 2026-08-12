using Xunit;
using ERP.Domain.Inventory.Entities;
using ERP.Domain.Inventory.Enums;

namespace ERP.Domain.UnitTests;

/// <summary>
/// Unit tests for StockItem entity
/// </summary>
public class StockItemTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateStockItem()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var name = "Test Product";
        var uomId = Guid.NewGuid();

        // Act
        var item = StockItem.Create(orgId, name, uomId);

        // Assert
        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal(orgId, item.OrganizationId);
        Assert.Equal(name, item.Name);
        Assert.Equal(uomId, item.UnitOfMeasureId);
        Assert.True(item.IsActive);
        Assert.False(item.IsDeleted);
        Assert.Equal(ValuationMethod.AverageCost, item.ValuationMethod);
    }

    [Fact]
    public void Create_WithAllParameters_ShouldSetProperties()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var uomId = Guid.NewGuid();
        var code = "PROD001";
        var barcode = "1234567890";
        var standardCost = 100m;
        var standardPrice = 150m;

        // Act
        var item = StockItem.Create(
            orgId, "Product", uomId,
            code: code,
            barcode: barcode,
            standardCost: standardCost,
            standardPrice: standardPrice,
            valuationMethod: ValuationMethod.FIFO,
            reorderLevel: 10,
            minimumStock: 5);

        // Assert
        Assert.Equal(code, item.Code);
        Assert.Equal(barcode, item.Barcode);
        Assert.Equal(standardCost, item.StandardCost);
        Assert.Equal(standardPrice, item.StandardPrice);
        Assert.Equal(ValuationMethod.FIFO, item.ValuationMethod);
        Assert.Equal(10, item.ReorderLevel);
        Assert.Equal(5, item.MinimumStock);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var uomId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            StockItem.Create(orgId, "", uomId));
        Assert.Contains("Item name is required", exception.Message);
    }

    [Fact]
    public void TotalStock_WithNoWarehouses_ShouldReturnZero()
    {
        // Arrange
        var item = StockItem.Create(Guid.NewGuid(), "Product", Guid.NewGuid());

        // Act & Assert
        Assert.Equal(0, item.TotalStock);
    }

    [Fact]
    public void TotalStock_WithWarehouses_ShouldSumQuantities()
    {
        // Arrange
        var item = StockItem.Create(Guid.NewGuid(), "Product", Guid.NewGuid());
        var warehouse1 = StockItemWarehouse.Create(item.Id, Guid.NewGuid());
        var warehouse2 = StockItemWarehouse.Create(item.Id, Guid.NewGuid());
        warehouse1.SetInitialStock(100, 10);
        warehouse2.SetInitialStock(50, 10);

        item.AddWarehouse(warehouse1);
        item.AddWarehouse(warehouse2);

        // Act & Assert
        Assert.Equal(150, item.TotalStock);
    }

    [Fact]
    public void IsBelowReorderLevel_WhenStockBelowReorder_ShouldReturnTrue()
    {
        // Arrange
        var item = StockItem.Create(
            Guid.NewGuid(), "Product", Guid.NewGuid(),
            reorderLevel: 100);
        var warehouse = StockItemWarehouse.Create(item.Id, Guid.NewGuid());
        warehouse.SetInitialStock(50, 10);
        item.AddWarehouse(warehouse);

        // Act & Assert
        Assert.True(item.IsBelowReorderLevel);
    }

    [Fact]
    public void IsBelowReorderLevel_WhenStockAboveReorder_ShouldReturnFalse()
    {
        // Arrange
        var item = StockItem.Create(
            Guid.NewGuid(), "Product", Guid.NewGuid(),
            reorderLevel: 100);
        var warehouse = StockItemWarehouse.Create(item.Id, Guid.NewGuid());
        warehouse.SetInitialStock(150, 10);
        item.AddWarehouse(warehouse);

        // Act & Assert
        Assert.False(item.IsBelowReorderLevel);
    }

    [Fact]
    public void Update_ShouldUpdateFields()
    {
        // Arrange
        var item = StockItem.Create(Guid.NewGuid(), "Old Name", Guid.NewGuid());
        var newName = "New Name";
        var newCode = "NEW001";

        // Act
        item.Update(name: newName, code: newCode);

        // Assert
        Assert.Equal(newName, item.Name);
        Assert.Equal(newCode, item.Code);
    }

    [Fact]
    public void EnableSerialTracking_ShouldSetTrackSerialsTrue()
    {
        // Arrange
        var item = StockItem.Create(Guid.NewGuid(), "Product", Guid.NewGuid());

        // Act
        item.EnableSerialTracking();

        // Assert
        Assert.True(item.TrackSerials);
    }

    [Fact]
    public void EnableBatchTracking_ShouldSetTrackBatchTrue()
    {
        // Arrange
        var item = StockItem.Create(Guid.NewGuid(), "Product", Guid.NewGuid());

        // Act
        item.EnableBatchTracking();

        // Assert
        Assert.True(item.TrackBatch);
    }

    [Fact]
    public void SetValuationMethod_ShouldUpdateMethod()
    {
        // Arrange
        var item = StockItem.Create(Guid.NewGuid(), "Product", Guid.NewGuid());

        // Act
        item.SetValuationMethod(ValuationMethod.FIFO);

        // Assert
        Assert.Equal(ValuationMethod.FIFO, item.ValuationMethod);
    }
}

/// <summary>
/// Unit tests for StockItemWarehouse entity
/// </summary>
public class StockItemWarehouseTests
{
    [Fact]
    public void Create_ShouldInitializeWithZeroQuantity()
    {
        // Arrange
        var stockItemId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();

        // Act
        var stockWarehouse = StockItemWarehouse.Create(stockItemId, warehouseId);

        // Assert
        Assert.Equal(stockItemId, stockWarehouse.StockItemId);
        Assert.Equal(warehouseId, stockWarehouse.WarehouseId);
        Assert.Equal(0, stockWarehouse.Quantity);
        Assert.Equal(0, stockWarehouse.AverageCost);
        Assert.Equal(0, stockWarehouse.AvailableQuantity);
    }

    [Fact]
    public void SetInitialStock_ShouldSetQuantityAndCost()
    {
        // Arrange
        var stockWarehouse = StockItemWarehouse.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act
        stockWarehouse.SetInitialStock(100, 15.5m);

        // Assert
        Assert.Equal(100, stockWarehouse.Quantity);
        Assert.Equal(15.5m, stockWarehouse.AverageCost);
        Assert.NotNull(stockWarehouse.LastStockIn);
    }

    [Fact]
    public void AddStock_WithPositiveQuantity_ShouldIncreaseQuantity()
    {
        // Arrange
        var stockWarehouse = StockItemWarehouse.Create(Guid.NewGuid(), Guid.NewGuid());
        stockWarehouse.SetInitialStock(100, 10);

        // Act
        stockWarehouse.AddStock(50, 12);

        // Assert
        Assert.Equal(150, stockWarehouse.Quantity);
    }

    [Fact]
    public void AddStock_ShouldCalculateAverageCost()
    {
        // Arrange
        var stockWarehouse = StockItemWarehouse.Create(Guid.NewGuid(), Guid.NewGuid());
        stockWarehouse.SetInitialStock(100, 10); // Total value = 1000

        // Act
        stockWarehouse.AddStock(100, 20); // Add 100 @ 20 = 2000, Total = 3000 / 200

        // Assert
        Assert.Equal(15m, stockWarehouse.AverageCost);
    }

    [Fact]
    public void AddStock_WithZeroQuantity_ShouldThrowException()
    {
        // Arrange
        var stockWarehouse = StockItemWarehouse.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        Assert.Throws<ArgumentException>(() => stockWarehouse.AddStock(0, 10));
    }

    [Fact]
    public void AddStock_WithNegativeQuantity_ShouldThrowException()
    {
        // Arrange
        var stockWarehouse = StockItemWarehouse.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        Assert.Throws<ArgumentException>(() => stockWarehouse.AddStock(-10, 10));
    }

    [Fact]
    public void RemoveStock_ShouldDecreaseQuantity()
    {
        // Arrange
        var stockWarehouse = StockItemWarehouse.Create(Guid.NewGuid(), Guid.NewGuid());
        stockWarehouse.SetInitialStock(100, 10);

        // Act
        stockWarehouse.RemoveStock(30);

        // Assert
        Assert.Equal(70, stockWarehouse.Quantity);
    }

    [Fact]
    public void RemoveStock_MoreThanAvailable_ShouldThrowException()
    {
        // Arrange
        var stockWarehouse = StockItemWarehouse.Create(Guid.NewGuid(), Guid.NewGuid());
        stockWarehouse.SetInitialStock(100, 10);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => stockWarehouse.RemoveStock(150));
    }

    [Fact]
    public void Reserve_ShouldIncreaseReservedQuantity()
    {
        // Arrange
        var stockWarehouse = StockItemWarehouse.Create(Guid.NewGuid(), Guid.NewGuid());
        stockWarehouse.SetInitialStock(100, 10);

        // Act
        stockWarehouse.Reserve(20);

        // Assert
        Assert.Equal(20, stockWarehouse.ReservedQuantity);
        Assert.Equal(80, stockWarehouse.AvailableQuantity);
    }

    [Fact]
    public void Reserve_MoreThanAvailable_ShouldThrowException()
    {
        // Arrange
        var stockWarehouse = StockItemWarehouse.Create(Guid.NewGuid(), Guid.NewGuid());
        stockWarehouse.SetInitialStock(100, 10);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => stockWarehouse.Reserve(150));
    }

    [Fact]
    public void Unreserve_ShouldDecreaseReservedQuantity()
    {
        // Arrange
        var stockWarehouse = StockItemWarehouse.Create(Guid.NewGuid(), Guid.NewGuid());
        stockWarehouse.SetInitialStock(100, 10);
        stockWarehouse.Reserve(30);

        // Act
        stockWarehouse.Unreserve(10);

        // Assert
        Assert.Equal(20, stockWarehouse.ReservedQuantity);
        Assert.Equal(80, stockWarehouse.AvailableQuantity);
    }
}
