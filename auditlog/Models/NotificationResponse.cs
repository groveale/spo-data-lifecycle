using System;
using Newtonsoft.Json;

namespace groveale
{
    public class NotificationResponse
    {
        [JsonProperty("tenantId")]
        public Guid TenantId { get; set; }

        [JsonProperty("clientId")]
        public Guid ClientId { get; set; }

        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("contentId")]
        public string ContentId { get; set; }

        [JsonProperty("contentUri")]
        public string ContentUri { get; set; }

        [JsonProperty("contentCreated")]
        public DateTime ContentCreated { get; set; }

        [JsonProperty("contentExpiration")]
        public DateTime ContentExpiration { get; set; }
    }
}