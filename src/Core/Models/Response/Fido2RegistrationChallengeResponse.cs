using System.Collections.Generic;
using Bit.Core.Models.Data;
using Newtonsoft.Json;

namespace Bit.Core.Models.Response
{
    public class Fido2RegistrationChallengeResponse
    {
        [JsonProperty("challenge")]
        public string Challenge { get; set; }
        [JsonProperty("timeout")]
        public double Timeout { get; set; }
        [JsonProperty("rp")]
        public Fido2RP Rp { get; set; }
        [JsonProperty("user")]
        public Fido2User User { get; set; }
        [JsonProperty("pubKeyCredParams")]
        public List<Fido2PubKeyCredParam> PubKeyCredParams { get; set; }
        [JsonProperty("excludeCredentials")]
        public List<Fido2CredentialDescriptor> ExcludeCredentials { get; set; }
        [JsonProperty("authenticatorSelection")]
        public Fido2AuthenticatorSelection AuthenticatorSelection { get; set; }
        [JsonProperty("attestation", NullValueHandling = NullValueHandling.Ignore)]
        public object Attestation { get; set; }
        [JsonProperty("extensions", NullValueHandling = NullValueHandling.Ignore)]
        public object Extensions { get; set; }
    }
}
