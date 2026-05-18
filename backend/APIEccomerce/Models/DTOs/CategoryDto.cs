using System.Text.Json.Serialization;

namespace APIEccomerce.Models.DTOs
{
    // [COMENTARIO DE EDICIÓN]
    public class CategoryDto
    {  

        public int CategoryId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}
