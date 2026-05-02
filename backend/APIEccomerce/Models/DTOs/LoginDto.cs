using System.Text.Json.Serialization;

namespace APIEccomerce.Models.DTOs
{
    public class LoginDto
    {
        [JsonPropertyName("correo")]
        public string Email { get; set; } = null!;

        [JsonPropertyName("clave")]
        public string Password { get; set; } = null!;
    }
}
