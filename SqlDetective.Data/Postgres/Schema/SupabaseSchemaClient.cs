using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace SqlDetective.Data.Postgres.Schema
{
    public class SupabaseSchemaClient : ISupabaseSchemaClient
    {
        private readonly HttpClient r_HttpClient;
        private readonly SupabaseOptions r_Options;

        public SupabaseSchemaClient(HttpClient httpClient, IOptions<SupabaseOptions> options)
        {
            r_HttpClient = httpClient;
            r_Options = options.Value;
        }

        public async Task<JArray> CallRpcAsync(string functionName, JObject body, CancellationToken ct = default)
        {
            string url = $"{r_Options.Url}/rest/v1/rpc/{functionName}";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(body.ToString(), Encoding.UTF8, "application/json")
            };

            request.Headers.Add("apikey", r_Options.ApiKey);
            request.Headers.Add("Authorization", $"Bearer {r_Options.ApiKey}");
            request.Headers.Accept.ParseAdd("application/json");

            using HttpResponseMessage response = await r_HttpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync(ct);
            return JArray.Parse(json);
        }
    }
}
