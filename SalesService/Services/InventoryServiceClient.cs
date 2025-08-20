using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SalesService.Services;

public interface IInventoryServiceClient
{
    Task<bool> ValidateStockAsync(int productId, int quantity, CancellationToken cancellationToken);
}

public class InventoryServiceClient : IInventoryServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public InventoryServiceClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> ValidateStockAsync(int productId, int quantity, CancellationToken cancellationToken)
    {
        var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrWhiteSpace(token))
        {
            token = token.StartsWith("Bearer ") ? token.Substring("Bearer ".Length) : token;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

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
