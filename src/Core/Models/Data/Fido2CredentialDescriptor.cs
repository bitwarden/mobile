using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bit.Core.Models.Data
{
    public class Fido2CredentialDescriptor : Data
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("transports", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Transports { get; set; }
    }
}
