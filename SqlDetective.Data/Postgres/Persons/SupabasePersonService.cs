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
using SqlDetective.Domain.Persons;

namespace SqlDetective.Data.Postgres.Persons
{
    public class SupabasePersonService : IPersonService
    {
        private readonly HttpClient r_HttpClient;
        private readonly SupabaseOptions r_Options;
    private readonly IPersonCache r_Cache;

    private const string k_PersonsTable = "Persons";

        public SupabasePersonService(HttpClient httpClient, IOptions<SupabaseOptions> options, IPersonCache i_Cache)
        {
            r_HttpClient = httpClient;
            r_Options = options.Value;
            r_Cache = i_Cache;
        }


        public async Task<IReadOnlyList<PersonDto>> GetAllAsync(CancellationToken ct = default)
        {
      var cached = r_Cache.GetPersons();
      if (cached != null) return cached;
      
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
            r_Cache.Store(result);
            
            return result;
        }
    }
}
