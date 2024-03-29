﻿using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Bit.Core.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bit.Core.Services.EmailForwarders
{
    public class FirefoxRelayForwarder : BaseForwarder<ForwarderOptions>
    {
        protected override string RequestUri => "https://relay.firefox.com/api/v1/relayaddresses/";

        protected override void ConfigureHeaders(HttpRequestHeaders headers, ForwarderOptions options)
        {
            headers.Add("Authorization", $"Token {options.ApiKey}");
        }

        protected override Task<HttpContent> GetContentAsync(IApiService apiService, ForwarderOptions options)
        {
            return Task.FromResult<HttpContent>(new StringContent(
                JsonConvert.SerializeObject(
                new
                {
                    enabled = true,
                    description = "Generated by Bitwarden."
                }), Encoding.UTF8, "application/json"));
        }

        protected override string HandleResponse(JObject result)
        {
            return result["full_address"]?.ToString();
        }
    }
}
