using System.Text.Json.Serialization;

namespace APIEccomerce.Models.DTOs
{
    public class ProductResponseDto
    {
        [JsonPropertyName("idProducto")]
        public int Id { get; set; }

        [JsonPropertyName("nombre")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("descripcion")]
        public string? Description { get; set; }

        [JsonPropertyName("precio")]
        public decimal Price { get; set; }

        [JsonPropertyName("stock")]
        public int Stock { get; set; }

        [JsonPropertyName("imagenUrl")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("activo")]
        public bool IsActive { get; set; }

        [JsonPropertyName("fechaCreacion")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("idCategoria")]
        public int CategoryId { get; set; }

        [JsonPropertyName("categoriaNombre")]
        public string? CategoryName { get; set; }
    }
}