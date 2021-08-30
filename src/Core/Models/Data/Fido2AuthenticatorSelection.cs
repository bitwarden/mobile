using Newtonsoft.Json;

namespace Bit.Core.Models.Data
{
    public class Fido2AuthenticatorSelection : Data
    {
        [JsonProperty("authenticatorAttachment")]
        public string AuthenticatorAttachment { get; set; }
        [JsonProperty("userVerification")]
        public string UserVerification { get; set; }
        [JsonProperty("requireResidentKey")]
        public string RequireResidentKey { get; set; }
    }
}
