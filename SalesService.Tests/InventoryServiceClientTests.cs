using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SalesService.Services;
using Xunit;

namespace SalesService.Tests;

public class InventoryServiceClientTests
{
    private class StubHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;
        public StubHandler(HttpResponseMessage response) => _response = response;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(_response);
    }

    [Fact]
    public async Task ValidateStockAsync_ReturnsTrue_WhenQuantitySufficient()
    {
        var json = "{\"quantity\":5}";
        var handler = new StubHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) });
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://test") };
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var service = new InventoryServiceClient(client, accessor);
        var result = await service.ValidateStockAsync(1, 3, CancellationToken.None);
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateStockAsync_ReturnsFalse_WhenQuantityInsufficient()
    {
        var json = "{\"quantity\":2}";
        var handler = new StubHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) });
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://test") };
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var service = new InventoryServiceClient(client, accessor);
        var result = await service.ValidateStockAsync(1, 3, CancellationToken.None);
        Assert.False(result);
    }
}
