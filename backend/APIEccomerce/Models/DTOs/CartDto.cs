using System.Text.Json.Serialization;

namespace APIEccomerce.Models.DTOs
{
    public class CartDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("userId")]
        public int UserId { get; set; }


        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("items")]
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();

        [JsonPropertyName("total")]
        public decimal Total { get; set; }
    }
}
