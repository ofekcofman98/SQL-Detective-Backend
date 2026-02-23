using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using SqlDetective.ExecutionEngine.Domain.Models;
using SqlDetective.ExecutionEngine.Domain.Services;

namespace SqlDetective.ExecutionEngine.Data
{
    public class PostgresQueryExecutionService : IQueryExecutionService
    {
        private readonly string r_ConnectionString;
        private readonly ILogger<PostgresQueryExecutionService> r_Logger;

        public PostgresQueryExecutionService(
            IConfiguration configuration,
            ILogger<PostgresQueryExecutionService> logger)
        {
            r_ConnectionString = configuration.GetConnectionString("SqlDetectiveDatabase")
                ?? throw new InvalidOperationException("Missing connection string 'SqlDetectiveDatabase'");
            r_Logger = logger;
        }

        public async Task<QueryExecutionResponse> ExecuteAsync(
            string sessionKey, 
            string sql, 
            CancellationToken ct = default)
        {
            r_Logger.LogInformation(
                "[ExecutionEngine] SessionKey={SessionKey}, SQL:\n{Sql}", 
                sessionKey, 
                sql);

            if (string.IsNullOrWhiteSpace(sessionKey))
            {
                return new QueryExecutionResponse
                {
                    Success = false,
                    ErrorMessage = "Session key cannot be empty"
                };
            }

            if (string.IsNullOrWhiteSpace(sql))
            {
                return new QueryExecutionResponse
                {
                    Success = false,
                    ErrorMessage = "SQL cannot be empty"
                };
            }

            var result = new List<Dictionary<string, object>>();

            try
            {
                await using var conn = new NpgsqlConnection(r_ConnectionString);
                r_Logger.LogInformation("[ExecutionEngine] Opening connection...");
                await conn.OpenAsync(ct);

                // Set read-only transaction for security
                await using var transaction = await conn.BeginTransactionAsync(ct);
                
                // Execute SET TRANSACTION READ ONLY and dispose immediately
                await using (var setReadOnlyCmd = new NpgsqlCommand(
                    "SET TRANSACTION READ ONLY", 
                    conn, 
                    transaction))
                {
                    await setReadOnlyCmd.ExecuteNonQueryAsync(ct);
                }

                await using var cmd = new NpgsqlCommand(sql, conn, transaction);
                cmd.CommandTimeout = 30;

                await using (var reader = await cmd.ExecuteReaderAsync(ct))
                {
                    while (await reader.ReadAsync(ct))
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string columnName = reader.GetName(i);
                            object columnValue = reader.GetValue(i);
                            row[columnName] = (columnValue == DBNull.Value) ? null : columnValue;
                        }

                        r_Logger.LogDebug(
                            "[ExecutionEngine] Row: {RowContent}", 
                            string.Join(", ", row.Select(kv => $"{kv.Key}: {kv.Value}")));

                        result.Add(row);
                    }
                } // Reader is disposed here

                await transaction.CommitAsync(ct);

                return new QueryExecutionResponse
                {
                    Success = true,
                    Data = result
                };
            }
            catch (NpgsqlException ex)
            {
                r_Logger.LogError(
                    ex, 
                    "[ExecutionEngine] SQL Execution Error for session {SessionKey}", 
                    sessionKey);

                return new QueryExecutionResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                r_Logger.LogError(
                    ex, 
                    "[ExecutionEngine] Unexpected error for session {SessionKey}", 
                    sessionKey);

                return new QueryExecutionResponse
                {
                    Success = false,
                    ErrorMessage = "Internal server error"
                };
            }
        }
    }
}
