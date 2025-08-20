using Microsoft.EntityFrameworkCore;
using SalesService.Data;
using SalesService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? throw new InvalidOperationException("DATABASE_URL is not set.");

// Add services to the container.
builder.Services.AddDbContext<SalesDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddHttpClient<IInventoryServiceClient, InventoryServiceClient>(client =>
{
    var baseUrl = builder.Configuration["InventoryService:BaseUrl"] ?? "http://inventory-service";
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
