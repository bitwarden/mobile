﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Bit.Core.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bit.Core.Services.EmailForwarders
{
    public class ForwardEmailForwarderOptions : ForwarderOptions
    {
        public string DomainName { get; set; }
    }

    public class ForwardEmailForwarder : BaseForwarder<ForwardEmailForwarderOptions>
    {
        private readonly string _domain;
        protected override string RequestUri => $"https://api.forwardemail.net/v1/domains/{_domain}/aliases";

        public ForwardEmailForwarder(string domain)
        {
            _domain = domain;
        }

        protected override bool CanGenerate(ForwardEmailForwarderOptions options)
        {
            return !string.IsNullOrWhiteSpace(options.ApiKey) && !string.IsNullOrWhiteSpace(options.DomainName);
        }

        protected override void ConfigureHeaders(HttpRequestHeaders headers, ForwardEmailForwarderOptions options)
        {
            headers.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes(options.ApiKey + ":"))}");
        }

        protected override Task<HttpContent> GetContentAsync(IApiService apiService, ForwardEmailForwarderOptions options)
        {
            return Task.FromResult<HttpContent>(new StringContent(
                JsonConvert.SerializeObject(
                new
                {
                    description = "Generated by Bitwarden."
                }), Encoding.UTF8, "application/json"));
        }

        protected override string HandleResponse(JObject result)
        {
            return $"{result["name"]}@{result["domain"]?["name"] ?? _domain}";
        }
    }
}
