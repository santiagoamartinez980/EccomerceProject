using APIEccomerce.Models.DTOs;
using APIEccomerce.Services.Interfaces;
using System.Text.Json;

namespace APIEccomerce.Services
{
    public class GoogleGeocodingService : IGoogleGeocodingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<GoogleGeocodingService> _logger;

        public GoogleGeocodingService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GoogleGeocodingService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GoogleMaps:ApiKey"]
                          ?? throw new InvalidOperationException(
                              "GoogleMaps:ApiKey no configurada.");
            _logger = logger;
        }

        public async Task<GeocodingResultDto?> GeocodeAddressAsync(
            string addressLine, string city, string department)

        {
            
            // Construye la dirección completa que enviará a Google
            var  fullAddress = $"{addressLine}, {city}, {department}, Colombia";
            var encoded = Uri.EscapeDataString(fullAddress);
            var url = $"https://maps.googleapis.com/maps/api/geocode/json" +
                              $"?address={encoded}&key={_apiKey}&language=es&region=CO";

            _logger.LogInformation("Geocoding: {Address}", fullAddress);

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Google Geocoding respondió {Code}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Google devuelve status OK si encontró resultados
            var status = root.GetProperty("status").GetString();
            if (status != "OK")
            {
                _logger.LogWarning("Geocoding status: {Status} para {Address}", status, fullAddress);
                return null;
            }

            var result = root.GetProperty("results")[0];
            var location = result
                .GetProperty("geometry")
                .GetProperty("location");

            var lat = location.GetProperty("lat").GetDouble();
            var lng = location.GetProperty("lng").GetDouble();
            var formattedAddress = result.GetProperty("formatted_address").GetString() ?? string.Empty;
            var placeId = result.GetProperty("place_id").GetString() ?? string.Empty;

            
            var postalCode = string.Empty;
            foreach (var component in result.GetProperty("address_components").EnumerateArray())
            {
                var types = component.GetProperty("types");
                foreach (var type in types.EnumerateArray())
                {
                    if (type.GetString() == "postal_code")
                    {
                        postalCode = component
                            .GetProperty("long_name")
                            .GetString() ?? string.Empty;
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(postalCode)) break;
            }

            return new GeocodingResultDto
            {
                Latitude = lat,
                Longitude = lng,
                FormattedAddress = formattedAddress,
                PostalCode = postalCode,
                PlaceId = placeId
            };
        }
    }
}