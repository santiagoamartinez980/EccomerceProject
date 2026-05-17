using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace APIEccomerce.Models.DTOs
{
    public class CreateProductDto
    {

        [JsonPropertyName("nombre")]
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("descripcion")]
        [MaxLength(1000)]
        [Required]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("precio")]
        [Required]
        [Range(0.01, 100000000)]
        public decimal Price { get; set; }

        [JsonPropertyName("stock")]
        [Range(0, int.MaxValue)]
        [Required]
        public int Stock { get; set; }

        [JsonPropertyName("imagenUrl")]
        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        [JsonPropertyName("activo")]
        [Required]
        public bool IsActive { get; set; } = true;

        [JsonPropertyName("idCategoria")]
        [Required]
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
    }
}
