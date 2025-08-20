using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesService.Data;
using SalesService.Models;
using SalesService.Services;
using System.Collections.Generic;

namespace SalesService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly SalesDbContext _context;
    private readonly IInventoryServiceClient _inventoryClient;
    private readonly IRabbitMqPublisher _publisher;

    public OrdersController(SalesDbContext context, IInventoryServiceClient inventoryClient, IRabbitMqPublisher publisher)
    {
        _context = context;
        _inventoryClient = inventoryClient;
        _publisher = publisher;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetAll(CancellationToken cancellationToken)
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .ToListAsync(cancellationToken);

        return orders;
    }

    [HttpPost]
    public async Task<IActionResult> Create(Order order, CancellationToken cancellationToken)
    {
        foreach (var item in order.Items)
        {
            var valid = await _inventoryClient.ValidateStockAsync(item.ProductId, item.Quantity, cancellationToken);
            if (!valid)
            {
                return BadRequest(new { message = $"Insufficient stock for product {item.ProductId}" });
            }
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        _publisher.PublishOrderConfirmed(order);

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetById(int id, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order == null)
        {
            return NotFound();
        }

        return order;
    }
}
