using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace MyMinimalApi.Tests
{
    public class OrderTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        [Fact]
        public async Task GetOrder_ReturnsOk_WhenOrderExists()
        {
            int orderId = 1;
            var response = await _client.GetAsync($"/orders/{orderId}");
            response.EnsureSuccessStatusCode(); 
            var order = await response.Content.ReadFromJsonAsync<Order>();
            Assert.Equal(orderId, order.OrderId);
        }

        [Fact]
        public async Task GetOrder_ReturnsNotFound_WhenOrderDoesNotExist()
        {
            int orderId = 999; // Не существующий ID
            var response = await _client.GetAsync($"/orders/{orderId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
    public record Order(int OrderId, float Weight, DateTime? OrderTime, DateTime? ExpectedDeliveryTime, DateTime? DeliveryTime, int DistrictId, string? Ip);
    }

