using System.Text.Json.Serialization;

namespace APIEccomerce.Models.DTOs
{
    public class TokenDto
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = null!;
    }
}
