using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace APIEccomerce.Models.DTOs
{
    public class UserDto
    {
        [JsonPropertyName("nombre")]
        [Required(ErrorMessage = "El nombre es requerido")]
        public string FirstName { get; set; } = null!;

        [JsonPropertyName("apellidos")]
        [Required]
        public string LastName { get; set; } = null!;

        [JsonPropertyName("correo")]
        [Required]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        public string Email { get; set; } = null!;

        [JsonPropertyName("clave")]
        [Required]
        [MinLength(6, ErrorMessage = "Mínimo 6 caracteres")]
        public string Password { get; set; } = null!;
    }
}