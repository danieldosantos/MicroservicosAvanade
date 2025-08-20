using System.Text;
using System.Text.Json;
using InventoryService.Data;
using InventoryService.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
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
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            var queue = _configuration.GetValue<string>("RabbitMq:QueueName") ?? "sales";
            _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
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
                            product.Quantity -= message.QuantitySold;
                            await db.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing RabbitMQ message");
                }
            };
            var queue = _configuration.GetValue<string>("RabbitMq:QueueName") ?? "sales";
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
