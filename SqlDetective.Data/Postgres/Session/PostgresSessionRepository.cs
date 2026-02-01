using Microsoft.Extensions.Configuration;
using Npgsql;
using SqlDetective.Domain.Sessions.Data;
using SqlDetective.Domain.Sessions.Repository;
using System;
using Npgsql;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace SqlDetective.Data.Postgres.Session
{
  public class PostgresSessionRepository : ISessionRepository
  {
    private readonly string _connectionString;
    private readonly ILogger<PostgresSessionRepository> _logger;


    public PostgresSessionRepository(IConfiguration configuration, ILogger<PostgresSessionRepository> logger)
    {
      _logger = logger;

      _connectionString = configuration.GetConnectionString("SqlDetectiveDatabase")
          ?? throw new InvalidOperationException("Missing connection string");
    }

    public async Task<GameSession> CreateAsync(GameSession session, CancellationToken cancellationToken = default)
    {
      try
      {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        using var cmd = new NpgsqlCommand(
            "INSERT INTO sessions (id, key, pc_connected, mobile_connected) VALUES (@id, @key, @pc, @mobile)", conn);

        cmd.Parameters.AddWithValue("id", session.Id);
        cmd.Parameters.AddWithValue("key", session.Key);
        cmd.Parameters.AddWithValue("pc", session.PcConnected);
        cmd.Parameters.AddWithValue("mobile", session.MobileConnected);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
        return session;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"[DB ERROR] CreateSession failed: {ex.Message}");
        throw;
      }
    }

      public async Task<GameSession?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
      const string sql = "SELECT id, key, pc_connected, mobile_connected FROM sessions WHERE key = @key LIMIT 1";

      try
      {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("key", key);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
          var id = reader.GetGuid(0);
          var sessionKey = reader.GetString(1);

          return new GameSession(sessionKey);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Timeout or Error in GetByKeyAsync for key {Key}", key);
        throw;
      }

      return null;
    }

    public async Task UpdateAsync(GameSession session, CancellationToken cancellationToken = default)
    {
      using var conn = new NpgsqlConnection(_connectionString);
      await conn.OpenAsync(cancellationToken);

      using var cmd = new NpgsqlCommand(
          "UPDATE sessions SET mobile_connected = @mobile WHERE key = @key", conn);

      cmd.Parameters.AddWithValue("mobile", session.MobileConnected);
      cmd.Parameters.AddWithValue("key", session.Key);

      await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
  }
}
