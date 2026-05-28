namespace APIEccomerce.Models.DTOs
{
    public class GeocodingResultDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string FormattedAddress { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string PlaceId { get; set; } = string.Empty;
    }
}