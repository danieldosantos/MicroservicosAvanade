using System.Net.Http;
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
        var response = await _httpClient.GetAsync($"/api/inventory/{productId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (int.TryParse(content, out var available))
        {
            return available >= quantity;
        }

        return false;
    }
}
