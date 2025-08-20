using System;
using System.Text;
using System.Text.Json;
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

    public RabbitMqPublisher(IConfiguration configuration)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMq:Host"] ?? "localhost"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: "order-confirmed", durable: false, exclusive: false, autoDelete: false, arguments: null);
    }

    public void PublishOrderConfirmed(Order order)
    {
        var message = JsonSerializer.Serialize(order);
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(exchange: string.Empty, routingKey: "order-confirmed", basicProperties: null, body: body);
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}
