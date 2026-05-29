using System.Text.Json.Serialization;

namespace APIEccomerce.Models.DTOs
{
    public class CreateOrderDto
    {
        [JsonPropertyName("addressId")]
        public int AddressId { get; set; }

        [JsonPropertyName("items")]
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }
}
