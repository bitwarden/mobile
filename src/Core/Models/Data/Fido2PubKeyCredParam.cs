using Newtonsoft.Json;

namespace Bit.Core.Models.Data
{
    public class Fido2PubKeyCredParam : Data
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("alg")]
        public int Alg { get; set; }
    }
}
