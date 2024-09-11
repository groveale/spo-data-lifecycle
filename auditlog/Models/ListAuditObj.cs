using System;
using Newtonsoft.Json;

namespace groveale
{
    public class ListAuditObj
    {
        [JsonProperty("id")]
        public string AuditLogId { get; set; }

        [JsonProperty("listName")]
        public string ListName { get; set; }

        [JsonProperty("listUrl")]
        public string ListUrl { get; set; }

        [JsonProperty("objectId")]
        public string ObjectId { get; set; }

        [JsonProperty("listBaseTemplateType")]
        public string ListBaseTemplateType { get; set; }

        [JsonProperty("listBaseType")]
        public string ListBaseType { get; set; }

        [JsonProperty("creationTime")]
        public DateTime CreationTime { get; set; }
    }
}