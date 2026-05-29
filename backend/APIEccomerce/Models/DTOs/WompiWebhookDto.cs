using System.Text.Json.Serialization;

namespace APIEccomerce.Models.DTOs
{
    public class WompiWebhookDto
    {
        [JsonPropertyName("event")]
        public string Event { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public WompiWebhookData Data { get; set; } = null!;

        [JsonPropertyName("signature")]
        public WompiWebhookSignature Signature { get; set; } = null!;

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }
    }

    public class WompiWebhookData
    {
        [JsonPropertyName("transaction")]
        public WompiTransaction Transaction { get; set; } = null!;
    }

    public class WompiTransaction
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("reference")]
        public string Reference { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("amount_in_cents")]
        public long AmountInCents { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("payment_method_type")]
        public string? PaymentMethodType { get; set; }
    }

    public class WompiWebhookSignature
    {
        [JsonPropertyName("properties")]
        public List<string> Properties { get; set; } = new();

        [JsonPropertyName("checksum")]
        public string Checksum { get; set; } = string.Empty;
    }
}
