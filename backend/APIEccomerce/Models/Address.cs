namespace APIEccomerce.Models
{
    public class Address
    {
        public int AddressId { get; set; }

        public int UserId { get; set; }

        public string AddressLine { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public string Country { get; set; } = "Colombia";

        public string PostalCode { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string Phone { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public string? PlaceId { get; set; }

        public string? FormattedAddress { get; set; }

        public bool IsDefault { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navegación
        public User User { get; set; } = null!;
    }
}