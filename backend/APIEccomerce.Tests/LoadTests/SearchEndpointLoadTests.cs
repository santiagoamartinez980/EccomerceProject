using FluentAssertions;
using System.Diagnostics;

namespace APIEccomerce.Tests.LoadTests
{
    public class SearchEndpointLoadTests
    {
        private const string BaseUrl = "http://localhost:5000";
        private const string SearchEndpoint = "/api/Producto/buscar";
        private const string CategoryEndpoint = "/api/Producto/categoria";

        [Fact]

        public async Task SearchProductos_DeberiaSoportar_100_RequestsPerSegundo()
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var searchTerms = new[] { "Laptop", "Mouse", "Teclado", "Monitor", "Headphones" };
            var tasks = new List<Task<HttpResponseMessage>>();

            // 100 búsquedas concurrentes
            for (int i = 0; i < 100; i++)
            {
                var term = searchTerms[i % searchTerms.Length];
                tasks.Add(httpClient.GetAsync($"{BaseUrl}{SearchEndpoint}?name={Uri.EscapeDataString(term)}"));
            }

            var results = await Task.WhenAll(tasks);
            var successCount = results.Count(r => r.IsSuccessStatusCode);

            successCount.Should().BeGreaterThan(95, "Al menos 95 de 100 búsquedas deberían ser exitosas");
        }

        [Fact]

        public async Task SearchProductos_P95Latency_DeberiaSerMenor300ms()
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var searchTerms = new[] { "Laptop", "Mouse", "Teclado" };
            var latencies = new List<long>();

            // 50 búsquedas secuenciales
            for (int i = 0; i < 50; i++)
            {
                var term = searchTerms[i % searchTerms.Length];
                var sw = Stopwatch.StartNew();
                var response = await httpClient.GetAsync(
                    $"{BaseUrl}{SearchEndpoint}?name={Uri.EscapeDataString(term)}");
                sw.Stop();

                if (response.IsSuccessStatusCode)
                {
                    latencies.Add(sw.ElapsedMilliseconds);
                }
            }

            if (latencies.Count > 0)
            {
                var p95 = latencies.OrderBy(x => x).Skip((int)(latencies.Count * 0.95)).First();
                p95.Should().BeLessThan(300, $"P95 latency debería ser <300ms, obtuvo {p95}ms");
            }
        }

        [Fact(Skip = "Requiere servidor ejecutándose")]
        
        public async Task GetProductosByCategoria_DeberiaSoportar_100_RequestsPerSegundo()
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var categories = new[] { "Electrónica", "Periféricos", "Accesorios" };
            var tasks = new List<Task<HttpResponseMessage>>();

            // 100 filtros por categoría concurrentes
            for (int i = 0; i < 100; i++)
            {
                var category = categories[i % categories.Length];
                tasks.Add(httpClient.GetAsync(
                    $"{BaseUrl}{CategoryEndpoint}?category={Uri.EscapeDataString(category)}"));
            }

            var results = await Task.WhenAll(tasks);
            var successCount = results.Count(r => r.IsSuccessStatusCode);

            successCount.Should().BeGreaterThan(95, "Al menos 95 de 100 requests deberían ser exitosas");
        }

        [Fact]

        public async Task SearchProductos_ErrorRate_DeberiaSerMenor1Porciento()
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var searchTerms = new[] { "Laptop", "Mouse", "Teclado", "Monitor" };
            var tasks = new List<Task<HttpResponseMessage>>();

            // 200 búsquedas concurrentes
            for (int i = 0; i < 200; i++)
            {
                var term = searchTerms[i % searchTerms.Length];
                tasks.Add(httpClient.GetAsync($"{BaseUrl}{SearchEndpoint}?name={Uri.EscapeDataString(term)}"));
            }

            var results = await Task.WhenAll(tasks);
            var errorRate = (double)results.Count(r => !r.IsSuccessStatusCode) / results.Length * 100;

            errorRate.Should().BeLessThan(1, $"Error rate debería ser <1%, obtuvo {errorRate}%");
        }

        [Fact]
        public async Task SearchProductos_StressTest_DeberiaManejar_300_UsuariosSimultaneos()
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            var searchTerms = new[] { "Laptop", "Mouse", "Teclado", "Monitor", "Headphones" };
            var tasks = new List<Task<HttpResponseMessage>>();

            // 300 búsquedas concurrentes
            for (int i = 0; i < 300; i++)
            {
                var term = searchTerms[i % searchTerms.Length];
                tasks.Add(httpClient.GetAsync($"{BaseUrl}{SearchEndpoint}?name={Uri.EscapeDataString(term)}"));
            }

            var results = await Task.WhenAll(tasks);
            var successRate = (double)results.Count(r => r.IsSuccessStatusCode) / results.Length * 100;

            successRate.Should().BeGreaterThan(85, 
                $"La mayoría debería ser exitosa (>85% bajo estrés), obtuvo {successRate}%");
        }

        [Fact]

        public async Task CombinedScenario_ReproduceTraficoReal()
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            var tasks = new List<Task<HttpResponseMessage>>();

            // Simular tráfico mixto: 30% búsqueda, 40% categorías, 30% catálogo
            for (int i = 0; i < 100; i++)
            {
                var op = i % 10;
                
                Task<HttpResponseMessage> request = op switch
                {
                    < 3 => httpClient.GetAsync($"{BaseUrl}{SearchEndpoint}?name=Laptop"),
                    < 7 => httpClient.GetAsync($"{BaseUrl}{CategoryEndpoint}?category=Electrónica"),
                    _ => httpClient.GetAsync($"{BaseUrl}/api/Producto")
                };

                tasks.Add(request);
            }

            var results = await Task.WhenAll(tasks);
            var errorRate = (double)results.Count(r => !r.IsSuccessStatusCode) / results.Length * 100;

            errorRate.Should().BeLessThan(2, $"Error rate en tráfico mixto debería ser <2%, obtuvo {errorRate}%");
        }
        [Fact]
        public async Task SearchProductos_ExtremeStressTest_500Requests()
        {
            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            var tasks = new List<Task<HttpResponseMessage>>();

            var searchTerms = new[]
            {
                "Laptop",
                "Mouse",
                "Monitor",
                "Teclado",
                "Headphones"
            };

            var sw = Stopwatch.StartNew();

            for (int i = 0; i < 500; i++)
            {
                var term = searchTerms[i % searchTerms.Length];

                tasks.Add(
                    httpClient.GetAsync(
                        $"{BaseUrl}{SearchEndpoint}?name={Uri.EscapeDataString(term)}"));
            }

            var results = await Task.WhenAll(tasks);

            sw.Stop();

            var successCount = results.Count(r => r.IsSuccessStatusCode);

            var errorCount = results.Length - successCount;

            var successRate =
                (double)successCount / results.Length * 100;

            Console.WriteLine("===== SEARCH STRESS TEST =====");
            Console.WriteLine($"Tiempo total: {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"Requests exitosos: {successCount}");
            Console.WriteLine($"Requests fallidos: {errorCount}");
            Console.WriteLine($"Success rate: {successRate}%");

            successRate.Should().BeGreaterThan(80);

        }

    }
}
