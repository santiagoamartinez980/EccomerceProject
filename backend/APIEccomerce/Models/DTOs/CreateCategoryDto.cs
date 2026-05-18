using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace APIEccomerce.Models.DTOs
{
    // [COMENTARIO DE EDICIÓN] 
    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
    }
}
