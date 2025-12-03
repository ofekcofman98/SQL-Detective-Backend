using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using SqlDetective.Data.Postgres.Schema;
using SqlDetective.Domain.Cases.Data;
using SqlDetective.Domain.Cases.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Data.Postgres.Case
{
    public class SupabaseCaseService : ICaseService
    {
        private readonly HttpClient m_HttpClient;
        private readonly SupabaseOptions m_Options;

        private const string k_CrimeEvidenceTable = "CrimeEvidence";

        public SupabaseCaseService(HttpClient httpClient, IOptions<SupabaseOptions> options)
        {
            m_HttpClient = httpClient;
            m_Options = options.Value;
        }

        public async Task<CaseDto> GetCaseAsync(string caseId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(caseId))
            {
                throw new ArgumentException("caseId cannot be null or empty", nameof(caseId));
            }

            string url =
                $"{m_Options.Url}/rest/v1/{k_CrimeEvidenceTable}" +
                $"?case_id=eq.{Uri.EscapeDataString(caseId)}" +
                $"&select=case_id,victim_id";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("apikey", m_Options.ApiKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", m_Options.ApiKey);
            request.Headers.Accept.ParseAdd("application/json");

            using HttpResponseMessage response = await m_HttpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            string json = await response.Content.ReadAsStringAsync(ct);

            JArray array = JArray.Parse(json);
            if (array.Count == 0)
            {
                return null;
            }

            JObject first = (JObject)array[0];

            return new CaseDto
            {
                CaseId = first["case_id"]?.ToString(),
                VictimId = first["victim_id"]?.ToString()
            };
        }

    }
}
