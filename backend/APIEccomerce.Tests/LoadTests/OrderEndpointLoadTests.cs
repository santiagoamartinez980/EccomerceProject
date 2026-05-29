using FluentAssertions;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace APIEccomerce.Tests.LoadTests
{
    public class OrderEndpointLoadTests
    {
        private const string BaseUrl = "http://localhost:5000";
        private const string OrderEndpoint = "/api/Order";
        private const string AuthToken = ""; // Reemplazar con token válido

        [Fact]
        public async Task CreateOrder_DeberiaSoportar_50_OrderesPerSegundo()
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {AuthToken}");

            var tasks = new List<Task<HttpResponseMessage>>();

            // 50 órdenes concurrentes
            for (int i = 0; i < 50; i++)
            {
                var orderDto = new
                {
                    addressId = 1,
                    items = new[] { new { productId = 1, quantity = 1 } }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(orderDto),
                    Encoding.UTF8,
                    "application/json");

                tasks.Add(httpClient.PostAsync($"{BaseUrl}{OrderEndpoint}", content));
            }

            var results = await Task.WhenAll(tasks);
            var successCount = results.Count(r => r.IsSuccessStatusCode);

            successCount.Should().BeGreaterThan(40, "Al menos 40 de 50 órdenes deberían ser exitosas");
        }

        [Fact]
        public async Task CreateOrder_P95Latency_DeberiaSerMenor800ms()
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {AuthToken}");

            var latencies = new List<long>();

            // 30 órdenes secuenciales para medir latencia
            for (int i = 0; i < 30; i++)
            {
                var orderDto = new
                {
                    addressId = 1,
                    items = new[] { new { productId = 1, quantity = 1 } }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(orderDto),
                    Encoding.UTF8,
                    "application/json");

                var sw = Stopwatch.StartNew();
                var response = await httpClient.PostAsync($"{BaseUrl}{OrderEndpoint}", content);
                sw.Stop();

                if (response.IsSuccessStatusCode)
                {
                    latencies.Add(sw.ElapsedMilliseconds);
                }
            }

            if (latencies.Count > 0)
            {
                var p95 = latencies.OrderBy(x => x).Skip((int)(latencies.Count * 0.95)).FirstOrDefault();
                p95.Should().BeLessThan(800, $"P95 latency debería ser <800ms, obtuvo {p95}ms");
            }
        }

        [Fact]
        public async Task CreateOrder_ErrorRate_DeberiaSerMenor5Porciento()
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {AuthToken}");

            var tasks = new List<Task<HttpResponseMessage>>();

            // 50 órdenes concurrentes
            for (int i = 0; i < 50; i++)
            {
                var orderDto = new
                {
                    addressId = 1,
                    items = new[] { new { productId = 1, quantity = 1 } }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(orderDto),
                    Encoding.UTF8,
                    "application/json");

                tasks.Add(httpClient.PostAsync($"{BaseUrl}{OrderEndpoint}", content));
            }

            var results = await Task.WhenAll(tasks);
            var errorRate = (double)results.Count(r => !r.IsSuccessStatusCode) / results.Length * 100;

            errorRate.Should().BeLessThan(5, $"Error rate debería ser <5%, obtuvo {errorRate}%");
        }

        [Fact]
        public async Task GetOrder_DeberiaSoportar_100_RequestsPerSegundo()
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {AuthToken}");

            var tasks = new List<Task<HttpResponseMessage>>();

            // 100 GET requests concurrentes
            for (int i = 0; i < 100; i++)
            {
                tasks.Add(httpClient.GetAsync($"{BaseUrl}{OrderEndpoint}"));
            }

            var results = await Task.WhenAll(tasks);
            var successCount = results.Count(r => r.IsSuccessStatusCode);

            successCount.Should().BeGreaterThan(95, "Al menos 95 de 100 GET requests deberían ser exitosas");
        }
    }
}
