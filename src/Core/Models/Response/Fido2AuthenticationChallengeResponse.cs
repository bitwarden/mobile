using System.Collections.Generic;
using Bit.Core.Models.Data;
using Newtonsoft.Json;

namespace Bit.Core.Models.Response
{
    public class Fido2AuthenticationChallengeResponse
    {
        [JsonProperty("challenge")]
        public string Challenge { get; set; }
        [JsonProperty("rpId")]
        public string RpId { get; set; }
        [JsonProperty("timeout")]
        public double Timeout { get; set; }
        [JsonProperty("allowCredentials")]
        public List<Fido2CredentialDescriptor> AllowCredentials { get; set; }
        [JsonProperty("userVerification")]
        public string UserVerification { get; set; }
        [JsonProperty("extensions", NullValueHandling = NullValueHandling.Ignore)]
        public object Extensions { get; set; }
    }
}
