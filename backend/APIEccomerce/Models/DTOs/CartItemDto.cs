using System.Text.Json.Serialization;

namespace APIEccomerce.Models.DTOs
{
    public class CartItemDto
    {
        [JsonPropertyName("cartItemId")]
        public int CartItemId { get; set; }

        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("productName")]
        public string ProductName { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("unitPrice")]
        public decimal UnitPrice { get; set; }

        [JsonPropertyName("subtotal")]
        public decimal Subtotal { get; set; }

        [JsonPropertyName("imagenUrl")]
        public string? ImagenUrl { get; set; }

        [JsonPropertyName("stock")]
        public int Stock { get; set; }
    }
}
