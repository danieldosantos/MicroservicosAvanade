using System;
using InventoryService.Controllers;
using InventoryService.Data;
using InventoryService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryService.Tests;

public class ProductsControllerTests
{
    private static InventoryDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new InventoryDbContext(options);
    }

    [Fact]
    public async Task Create_Product_PersistsAndReturnsCreated()
    {
        await using var db = CreateDbContext();
        var controller = new ProductsController(db);
        var product = new Product { Name = "Item", Price = 10m, Quantity = 5 };
        var result = await controller.Create(product);
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returned = Assert.IsType<Product>(created.Value);
        Assert.Equal("Item", returned.Name);
        Assert.Equal(1, await db.Products.CountAsync());
    }
}
