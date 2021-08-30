using Newtonsoft.Json;

namespace Bit.Core.Models.Data
{
    public class Fido2AssertionResponse : Data
    {
        [JsonProperty("authenticatorData")]
        public string AuthenticatorData { get; set; }
        [JsonProperty("signature")]
        public string Signature { get; set; }
        [JsonProperty("clientDataJson")]
        public string ClientDataJson { get; set; }
        [JsonProperty("userHandle")]
        public string UserHandle { get; set; }
    }
}
