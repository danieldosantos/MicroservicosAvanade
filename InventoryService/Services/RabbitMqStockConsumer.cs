using System.Text;
using System.Text.Json;
using InventoryService.Data;
using InventoryService.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace InventoryService.Services
{
    public class RabbitMqStockConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RabbitMqStockConsumer> _logger;
        private readonly IConfiguration _configuration;
        private IConnection? _connection;
        private IModel? _channel;

        public RabbitMqStockConsumer(IServiceProvider serviceProvider, ILogger<RabbitMqStockConsumer> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration.GetValue<string>("RabbitMq:Host") ?? "localhost"
            };
            _logger.LogInformation("Connecting to RabbitMQ at {Host}", factory.HostName);
            _connection = factory.CreateConnection();
            _logger.LogInformation("RabbitMQ connection established");
            _channel = _connection.CreateModel();
            var queue = _configuration.GetValue<string>("RabbitMq:QueueName") ?? "sales";
            _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _logger.LogInformation("Declared queue {QueueName}", queue);
            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_channel == null)
            {
                return Task.CompletedTask;
            }

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                _logger.LogInformation("Received message: {Message}", messageJson);
                try
                {
                    var message = JsonSerializer.Deserialize<StockAdjustmentMessage>(messageJson);
                    if (message != null)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
                        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == message.ProductId);
                        if (product != null)
                        {
                            _logger.LogInformation("Adjusting stock for Product {ProductId} by {Quantity}", message.ProductId, message.QuantitySold);
                            product.Quantity -= message.QuantitySold;
                            await db.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation("Stock adjusted for Product {ProductId}. New quantity: {Quantity}", product.Id, product.Quantity);
                        }
                        else
                        {
                            _logger.LogWarning("Product {ProductId} not found while processing stock adjustment", message.ProductId);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Received invalid StockAdjustmentMessage: {Message}", messageJson);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing RabbitMQ message");
                }
            };
            var queue = _configuration.GetValue<string>("RabbitMq:QueueName") ?? "sales";
            _logger.LogInformation("Starting consumer on queue {QueueName}", queue);
            _channel.BasicConsume(queue, autoAck: true, consumer: consumer);
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
