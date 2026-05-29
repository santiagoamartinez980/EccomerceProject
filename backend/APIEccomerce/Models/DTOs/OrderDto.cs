using System.Text.Json.Serialization;

namespace APIEccomerce.Models.DTOs
{
    public class OrderDto
    {
        [JsonPropertyName("orderId")]
        public int OrderId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("total")]
        public decimal Total { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("address")]
        public AddressDto Address { get; set; } = null!;

        [JsonPropertyName("details")]
        public List<OrderDetailDto> Details { get; set; } = new();
    }

    
}
