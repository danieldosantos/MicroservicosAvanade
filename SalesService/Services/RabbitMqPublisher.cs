using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
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
        _queueName = configuration["RabbitMq:QueueName"] ?? "order-confirmed";
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMq:Host"] ?? "localhost",
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
        var message = JsonSerializer.Serialize(order);
        var body = Encoding.UTF8.GetBytes(message);

        const int maxRetries = 3;
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation("Publishing message to {Queue} (attempt {Attempt})", _queueName, attempt);
                _channel.BasicPublish(exchange: string.Empty, routingKey: _queueName, basicProperties: null, body: body);
                _logger.LogInformation("Message published to {Queue}", _queueName);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish message to {Queue} on attempt {Attempt}", _queueName, attempt);
                if (attempt == maxRetries)
                {
                    throw;
                }
                Thread.Sleep(1000);
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
