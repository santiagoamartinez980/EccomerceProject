using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace APIEccomerce.Models.DTOs
{
    public class UpdateProductDto
    {
        [JsonPropertyName("nombre")]
        [MaxLength(50)]
        public string? Name { get; set; }

        [JsonPropertyName("descripcion")]
        [MaxLength(100)]
        public string? Description { get; set; }

        [JsonPropertyName("precio")]
        [Range(0.01, 100000000)]
        public decimal? Price { get; set; }

        [JsonPropertyName("stock")]
        [Range(0, int.MaxValue)]
        public int? Stock { get; set; }

        [JsonPropertyName("imagenUrl")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("activo")]
        public bool? IsActive { get; set; }

        [JsonPropertyName("idCategoria")]
        [Range(1, int.MaxValue)]
        public int? CategoryId { get; set; }
    }
}
