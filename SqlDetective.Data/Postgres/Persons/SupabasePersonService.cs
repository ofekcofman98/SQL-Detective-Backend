using Microsoft.Extensions.Options;
using SqlDetective.Data.Postgres.Schema;
using SqlDetective.Domain.Persons.Data;
using SqlDetective.Domain.Persons.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SqlDetective.Data.Postgres.Persons
{
    public class SupabasePersonService : IPersonService
    {
        private readonly HttpClient r_HttpClient;
        private readonly SupabaseOptions r_Options;

        private const string k_PersonsTable = "Persons";

        public SupabasePersonService(HttpClient httpClient, IOptions<SupabaseOptions> options)
        {
            r_HttpClient = httpClient;
            r_Options = options.Value;
        }


        public async Task<IReadOnlyList<PersonDto>> GetAllAsync(CancellationToken ct = default)
        {
            string url = $"{r_Options.Url}/rest/v1/{k_PersonsTable}?select=*";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("apikey", r_Options.ApiKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", r_Options.ApiKey);
            request.Headers.Accept.ParseAdd("application/json");

            using HttpResponseMessage response = await r_HttpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                return Array.Empty<PersonDto>();
            }

            string json = await response.Content.ReadAsStringAsync(ct);
            JArray array = JArray.Parse(json);

            List<PersonDto> result = new List<PersonDto>(array.Count);

            foreach (JObject obj in array)
            {
                result.Add(new PersonDto
                {
                    Id = obj["person_id"]?.ToString(),
                    FirstName = obj["first_name"]?.ToString(),
                    LastName = obj["last_name"]?.ToString(),
                    PhotoUrl = obj["photo_url"]?.ToString(),
                    PrefabId = obj["prefab_id"]?.ToString()
                });
            }

            return result;
        }
    }
}
