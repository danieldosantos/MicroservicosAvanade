using Microsoft.EntityFrameworkCore;
using SalesService.Data;
using SalesService.Services;

var builder = WebApplication.CreateBuilder(args);

// Carrega variáveis de ambiente
builder.Configuration.AddEnvironmentVariables();

// String de conexão: prioriza env var e cai para appsettings.json
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DATABASE_URL is not set.");

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
