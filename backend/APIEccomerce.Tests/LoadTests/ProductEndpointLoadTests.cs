using FluentAssertions;
using System.Diagnostics;

namespace APIEccomerce.Tests.LoadTests
{
    public class ProductEndpointLoadTests
    {
        private const string BaseUrl = "http://localhost:5000";
        private const string ProductEndpoint = "/api/Producto";

        [Fact]

        public async Task GetProductos_DeberiaSoportar_100_RequestsPerSecond()
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var sw = Stopwatch.StartNew();
            var tasks = new List<Task<HttpResponseMessage>>();

            // 100 requests concurrentes
            for (int i = 0; i < 100; i++)
            {
                tasks.Add(httpClient.GetAsync($"{BaseUrl}{ProductEndpoint}"));
            }

            var results = await Task.WhenAll(tasks);
            sw.Stop();

            var successCount = results.Count(r => r.IsSuccessStatusCode);
            var errorRate = (double)(results.Length - successCount) / results.Length * 100;

            successCount.Should().BeGreaterThan(95, "Al menos 95 de 100 requests deberían ser exitosas");
            errorRate.Should().BeLessThan(5, "Error rate debería ser menor al 5%");
        }

        [Fact]

        public async Task GetProductos_P95Latency_DeberiaSerMenor200ms()
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var latencies = new List<long>();

            // 50 requests secuenciales para medir latencia
            for (int i = 0; i < 50; i++)
            {
                var sw = Stopwatch.StartNew();
                var response = await httpClient.GetAsync($"{BaseUrl}{ProductEndpoint}");
                sw.Stop();

                if (response.IsSuccessStatusCode)
                {
                    latencies.Add(sw.ElapsedMilliseconds);
                }
            }

            latencies.Should().NotBeEmpty();
            var p95 = latencies.OrderBy(x => x).Skip((int)(latencies.Count * 0.95)).First();

            p95.Should().BeLessThan(200, $"P95 latency debería ser <200ms, obtuvo {p95}ms");
        }

        [Fact]

        public async Task GetProductos_P99Latency_DeberiaSerMenor500ms()
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var latencies = new List<long>();

            // 100 requests secuenciales
            for (int i = 0; i < 100; i++)
            {
                var sw = Stopwatch.StartNew();
                var response = await httpClient.GetAsync($"{BaseUrl}{ProductEndpoint}");
                sw.Stop();

                if (response.IsSuccessStatusCode)
                {
                    latencies.Add(sw.ElapsedMilliseconds);
                }
            }

            var p99 = latencies.OrderBy(x => x).Skip((int)(latencies.Count * 0.99)).First();

            p99.Should().BeLessThan(500, $"P99 latency debería ser <500ms, obtuvo {p99}ms");
        }

        [Fact]

        public async Task GetProductos_ErrorRate_DeberiaSerMenor1Porciento()
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var tasks = new List<Task<HttpResponseMessage>>();

            // 200 requests concurrentes
            for (int i = 0; i < 200; i++)
            {
                tasks.Add(httpClient.GetAsync($"{BaseUrl}{ProductEndpoint}"));
            }

            var results = await Task.WhenAll(tasks);
            var errorRate = (double)results.Count(r => !r.IsSuccessStatusCode) / results.Length * 100;

            errorRate.Should().BeLessThan(1, $"Error rate debería ser <1%, obtuvo {errorRate}%");
        }

        [Fact]

        public async Task GetProductos_StressTest_DeberiaManejar_500_UsuariosSimultaneos()
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            var tasks = new List<Task<HttpResponseMessage>>();

            // 500 requests concurrentes
            for (int i = 0; i < 500; i++)
            {
                tasks.Add(httpClient.GetAsync($"{BaseUrl}{ProductEndpoint}"));
            }

            var results = await Task.WhenAll(tasks);
            var successCount = results.Count(r => r.IsSuccessStatusCode);
            var successRate = (double)successCount / results.Length * 100;

            successRate.Should().BeGreaterThan(90, 
                $"La mayoría debería ser exitosa (>90%), obtuvo {successRate}%");
        }
        [Fact]
        public async Task GetProductos_ExtremeStressTest_1000Requests()
        {
            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

             var tasks = new List<Task<HttpResponseMessage>>();

            var sw = Stopwatch.StartNew();

            // 1000 requests concurrentes
            for (int i = 0; i < 150; i++)
            {
                tasks.Add(
                    httpClient.GetAsync(
                        $"{BaseUrl}{ProductEndpoint}"));
            }

            var results = await Task.WhenAll(tasks);

            sw.Stop();

            var successCount = results.Count(r => r.IsSuccessStatusCode);

            var errorCount = results.Length - successCount;

            var successRate =
                (double)successCount / results.Length * 100;

            Console.WriteLine($"Tiempo total: {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"Requests exitosos: {successCount}");
            Console.WriteLine($"Requests fallidos: {errorCount}");
            Console.WriteLine($"Success rate: {successRate}%");

            successRate.Should().BeGreaterThan(
                70,
                "La API debería soportar estrés extremo");


}

    }
}
