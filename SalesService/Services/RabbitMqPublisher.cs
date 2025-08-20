using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SalesService.Messages;
using SalesService.Models;

namespace SalesService.Services;

public interface IRabbitMqPublisher : IDisposable
{
    void PublishOrderConfirmed(Order order);
}

public class RabbitMqPublisher : IRabbitMqPublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly string _queueName;

    public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;
        _queueName = configuration.GetValue<string>("RabbitMq:QueueName") ?? "sales";
        var factory = new ConnectionFactory
        {
            HostName = configuration.GetValue<string>("RabbitMq:Host") ?? "localhost",
            AutomaticRecoveryEnabled = true
        };

        _logger.LogInformation("Connecting to RabbitMQ at {Host}", factory.HostName);
        _connection = factory.CreateConnection();
        _logger.LogInformation("RabbitMQ connection established");
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        _logger.LogInformation("Declared queue {QueueName}", _queueName);
    }

    public void PublishOrderConfirmed(Order order)
    {
        _logger.LogInformation("Publishing stock adjustments for Order {OrderId} with {ItemCount} items", order.Id, order.Items.Count);
        foreach (var item in order.Items)
        {
            var message = new StockAdjustmentMessage
            {
                ProductId = item.ProductId,
                QuantitySold = item.Quantity
            };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            const int maxRetries = 3;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation("Publishing stock adjustment for Product {ProductId} to {Queue} (attempt {Attempt})", item.ProductId, _queueName, attempt);
                    _channel.BasicPublish(exchange: string.Empty, routingKey: _queueName, basicProperties: null, body: body);
                    _logger.LogInformation("Stock adjustment for Product {ProductId} published to {Queue}", item.ProductId, _queueName);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish stock adjustment for Product {ProductId} to {Queue} on attempt {Attempt}", item.ProductId, _queueName, attempt);
                    if (attempt == maxRetries)
                    {
                        throw;
                    }
                    Thread.Sleep(1000);
                }
            }
        }
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing RabbitMQ connection");
        _channel.Dispose();
        _connection.Dispose();
        _logger.LogInformation("RabbitMQ connection disposed");
    }
}
