using APIEccomerce.Models.DTOs;

namespace APIEccomerce.Services.Interfaces
{
    public interface IGoogleGeocodingService
    {
        Task<GeocodingResultDto?> GeocodeAddressAsync(string addressLine, string city, string department);
    }
}