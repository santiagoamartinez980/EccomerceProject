using System.Text.Json.Serialization;

namespace APIEccomerce.Models.DTOs
{
    public class PaymentIntentDto
    {
        [JsonPropertyName("reference")]
        public string Reference { get; set; } = string.Empty;

        [JsonPropertyName("amountInCents")]
        public long AmountInCents { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "COP";

        [JsonPropertyName("publicKey")]
        public string PublicKey { get; set; } = string.Empty;

        [JsonPropertyName("signature")]
        public string Signature { get; set; } = string.Empty;

        [JsonPropertyName("redirectUrl")]
        public string RedirectUrl { get; set; } = string.Empty;
    }
}
