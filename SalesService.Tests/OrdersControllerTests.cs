using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesService.Controllers;
using SalesService.Data;
using SalesService.Models;
using SalesService.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SalesService.Tests;

public class OrdersControllerTests
{
    private static SalesDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<SalesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new SalesDbContext(options);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenStockValid()
    {
        await using var db = CreateDb();
        var inventory = new StubInventory(true);
        var publisher = new StubPublisher();
        var controller = new OrdersController(db, inventory, publisher);
        var order = new Order
        {
            Items = new List<OrderItem> { new OrderItem { ProductId = 1, Quantity = 1, UnitPrice = 10m } }
        };
        var result = await controller.Create(order, CancellationToken.None);
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(1, await db.Orders.CountAsync());
        Assert.Single(publisher.Published);
    }

    private sealed class StubInventory : IInventoryServiceClient
    {
        private readonly bool _valid;
        public StubInventory(bool valid) => _valid = valid;
        public Task<bool> ValidateStockAsync(int productId, int quantity, CancellationToken cancellationToken) => Task.FromResult(_valid);
    }

    private sealed class StubPublisher : IRabbitMqPublisher
    {
        public List<Order> Published { get; } = new();
        public void PublishOrderConfirmed(Order order) => Published.Add(order);
    }
}
