namespace APIEccomerce.Models.DTOs
{
    public class CreateAddressDto
    {
        public string AddressLine { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public string Country { get; set; } = "Colombia";

        public string PostalCode { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string? PlaceId { get; set; }

        public string? FormattedAddress { get; set; }

        public bool IsDefault { get; set; }
    }
}