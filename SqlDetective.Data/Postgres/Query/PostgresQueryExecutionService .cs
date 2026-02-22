using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using SqlDetective.Domain.Query.Service;
using SqlDetective.Domain.Sessions.Repository;

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

    public async Task<List<Dictionary<string, object>>> ExecuteAsync(string sessionKey, string sql, CancellationToken ct = default)
    {
      r_Logger.LogInformation("[QueryExecution] SessionKey={SessionKey}, SQL:\n{Sql}", sessionKey, sql);

      if (string.IsNullOrWhiteSpace(sessionKey)) throw new ArgumentException("sessionKey cannot be empty");
      if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentException("sql cannot be empty");

      var session = await r_SessionRepository.GetByKeyAsync(sessionKey, ct);
      if (session == null) throw new InvalidOperationException($"Session {sessionKey} not found");

      var result = new List<Dictionary<string, object>>();

      try
      {
        await using var conn = new NpgsqlConnection(r_ConnectionString);
        r_Logger.LogInformation("[DB] Opening connection...");
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.CommandTimeout = 30;

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
          var row = new Dictionary<string, object>();
          for (int i = 0; i < reader.FieldCount; i++)
          {
            string columnName = reader.GetName(i);
            object columnValue = reader.GetValue(i);

            row[columnName] = (columnValue == DBNull.Value) ? null : columnValue;
          }

          r_Logger.LogInformation("[DB_RESULT_ROW] {RowContent}", string.Join(", ", row.Select(kv => $"{kv.Key}: {kv.Value}")));

          result.Add(row);
        }
      }
      catch (NpgsqlException ex)
      {
        r_Logger.LogError(ex, "SQL Execution Error for session {SessionKey}", sessionKey);

        result.Add(new Dictionary<string, object> { { "error", ex.Message } });
      }

      return result;
    }

    //public async Task<JArray> ExecuteAsync(string sessionKey, string sql, CancellationToken ct = default)
    //{
    //    r_Logger.LogInformation("[QueryExecution] SessionKey={SessionKey}, SQL:\n{Sql}", sessionKey, sql);

    //    if (string.IsNullOrWhiteSpace(sessionKey)) throw new ArgumentException("sessionKey cannot be empty");
    //    if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentException("sql cannot be empty");

    //    var session = await r_SessionRepository.GetByKeyAsync(sessionKey, ct);

    //    if (session == null) throw new InvalidOperationException($"Session {sessionKey} not found");

    //    var result = new JArray();

    //    try
    //    {
    //      await using var conn = new NpgsqlConnection(r_ConnectionString);
    //      r_Logger.LogInformation("[DB] Opening connection...");
    //      await conn.OpenAsync(ct);

    //      await using var cmd = new NpgsqlCommand(sql, conn);
    //      cmd.CommandTimeout = 30;

    //      await using var reader = await cmd.ExecuteReaderAsync(ct);

    //      while (await reader.ReadAsync(ct))
    //      {
    //        var obj = new JObject();
    //        for (int i = 0; i < reader.FieldCount; i++)
    //        {
    //          string name = reader.GetName(i);
    //          object value = reader.GetValue(i);

    //          if (value == null || value == DBNull.Value)
    //          {
    //            obj[name] = JValue.CreateNull();
    //          }
    //          else if (value is Guid guidValue)
    //          {
    //            obj[name] = guidValue.ToString();
    //          }
    //          else
    //          {
    //            obj[name] = JToken.FromObject(value);
    //          }
    //        }
    //  r_Logger.LogInformation("[DB_RESULT_ROW] {RowJson}", obj.ToString(Newtonsoft.Json.Formatting.None));

    //  result.Add(obj);
    //      }
    //    }
    //    catch (NpgsqlException ex)
    //    {
    //      r_Logger.LogError(ex, "SQL Execution Error for session {SessionKey}", sessionKey);
    //      var errorObj = new JObject { ["error"] = ex.Message };
    //      result.Add(errorObj);
    //    }

    //    return result;
    //  }
  }
}
