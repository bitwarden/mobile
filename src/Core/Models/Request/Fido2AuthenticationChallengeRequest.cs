using Bit.Core.Models.Data;
using Newtonsoft.Json;

namespace Bit.Core.Models.Request
{
    public class Fido2AuthenticationChallengeRequest
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("rawId")]
        public string RawId { get; set; }
        [JsonProperty("response")]
        public Fido2AssertionResponse Response { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("extensions", NullValueHandling = NullValueHandling.Ignore)]
        public string Extensions { get; set; }
    }
}
