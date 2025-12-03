using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Npgsql;
using SqlDetective.Domain.Query.Service;
using SqlDetective.Domain.Sessions.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Data.Postgres.Query
{
    public class PostgresQueryExecutionService : IQueryExecutionService
    {
        private readonly string r_ConnectionString;
        private readonly ISessionRepository r_SessionRepository;

        public PostgresQueryExecutionService(
            IConfiguration configuration,
            ISessionRepository sessionRepository)
        {
            r_ConnectionString = configuration.GetConnectionString("SqlDetectiveDatabase")
                ?? throw new InvalidOperationException("Missing connection string 'SqlDetectiveDatabase'");
            r_SessionRepository = sessionRepository;
        }

        public async Task<JArray> ExecuteAsync(string sessionKey, string sql, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(sessionKey))
            {
                throw new ArgumentException("sessionKey cannot be empty", nameof(sessionKey));
            }

            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException("sql cannot be empty", nameof(sql));
            }

            var session = await r_SessionRepository.GetByKeyAsync(sessionKey, ct);
            if (session == null)
            {
                throw new InvalidOperationException($"Session with key {sessionKey} was not found");
            }

            using var conn = new NpgsqlConnection(r_ConnectionString);
            await conn.OpenAsync(ct);

            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync(ct);

            var result = new JArray();

            while (await reader.ReadAsync(ct))
            {
                var obj = new JObject();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string name = reader.GetName(i);
                    object value = reader.IsDBNull(i) ? null : reader.GetValue(i);

                    obj[name] = value == null ? JValue.CreateNull() : JToken.FromObject(value);
                }

                result.Add(obj);
            }

            return result;
        }
    }
}
