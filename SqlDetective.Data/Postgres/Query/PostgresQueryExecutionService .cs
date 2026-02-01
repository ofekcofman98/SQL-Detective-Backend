using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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

        private readonly ILogger<PostgresQueryExecutionService> r_Logger;

        public PostgresQueryExecutionService(
            IConfiguration configuration,
            ISessionRepository sessionRepository,
            ILogger<PostgresQueryExecutionService> logger)
        {
            r_ConnectionString = configuration.GetConnectionString("SqlDetectiveDatabase")
                ?? throw new InvalidOperationException("Missing connection string 'SqlDetectiveDatabase'");
            r_SessionRepository = sessionRepository;
            r_Logger = logger;
        }

        public async Task<JArray> ExecuteAsync(string sessionKey, string sql, CancellationToken ct = default)
        {
            r_Logger.LogInformation("[QueryExecution] SessionKey={SessionKey}, SQL:\n{Sql}", sessionKey, sql);

            if (string.IsNullOrWhiteSpace(sessionKey)) throw new ArgumentException("sessionKey cannot be empty");
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentException("sql cannot be empty");
 
            var session = await r_SessionRepository.GetByKeyAsync(sessionKey, ct);

            if (session == null) throw new InvalidOperationException($"Session {sessionKey} not found");

            var result = new JArray();

            try
            {
              await using var conn = new NpgsqlConnection(r_ConnectionString);
              await conn.OpenAsync(ct);

              await using var cmd = new NpgsqlCommand(sql, conn);
              cmd.CommandTimeout = 10;

              await using var reader = await cmd.ExecuteReaderAsync(ct);

              while (await reader.ReadAsync(ct))
              {
                var obj = new JObject();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                  string name = reader.GetName(i);
                  var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                  obj[name] = value != null ? JToken.FromObject(value) : JValue.CreateNull();
                }
                result.Add(obj);
              }
            }
            catch (NpgsqlException ex)
            {
              r_Logger.LogError(ex, "SQL Execution Error for session {SessionKey}", sessionKey);
              var errorObj = new JObject { ["error"] = ex.Message };
              result.Add(errorObj);
            }

            return result;
          }
    }
}
