using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SalesService.Services;

public interface IInventoryServiceClient
{
    Task<bool> ValidateStockAsync(int productId, int quantity, CancellationToken cancellationToken);
}

public class InventoryServiceClient : IInventoryServiceClient
{
    private readonly HttpClient _httpClient;

    public InventoryServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> ValidateStockAsync(int productId, int quantity, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"/products/{productId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        try
        {
            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var product = await JsonSerializer.DeserializeAsync<ProductDto>(stream, cancellationToken: cancellationToken);
            return product?.Quantity >= quantity;
        }
        catch
        {
            return false;
        }
    }

    private sealed class ProductDto
    {
        public int Quantity { get; set; }
    }
}
