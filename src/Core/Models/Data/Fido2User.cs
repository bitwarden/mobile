using Newtonsoft.Json;

namespace Bit.Core.Models.Data
{
    public class Fido2User : Data
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("icon")]
        public string Icon { get; set; }
    }
}
