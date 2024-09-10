using System.Text.Json.Serialization;

namespace groveale.Models
{
    public class ApiSubscription
    {
        [JsonPropertyName("contentType")]
        public string ContentType { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("webhook")]
        public Webhook? Webhook { get; set; }
    }

    public class Webhook
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("authId")]
        public string AuthId { get; set; }

        [JsonPropertyName("expiration")]
        public DateTime? Expiration { get; set; }
    }
}