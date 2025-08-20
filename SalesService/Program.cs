using Microsoft.EntityFrameworkCore;
using SalesService.Data;
using SalesService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SalesDbContext>(options =>
    options.UseSqlServer(connectionString));

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
