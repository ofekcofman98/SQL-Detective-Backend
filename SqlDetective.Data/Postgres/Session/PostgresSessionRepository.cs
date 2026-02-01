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

namespace SqlDetective.Data.Postgres.Session
{
  public class PostgresSessionRepository : ISessionRepository
  {
    private readonly string _connectionString;

    public PostgresSessionRepository(IConfiguration configuration)
    {
      _connectionString = configuration.GetConnectionString("SqlDetectiveDatabase")
          ?? throw new InvalidOperationException("Missing connection string");
    }

    public async Task<GameSession> CreateAsync(GameSession session, CancellationToken cancellationToken = default)
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

    public async Task<GameSession?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
      await using var conn = new NpgsqlConnection(_connectionString);
      await conn.OpenAsync(cancellationToken);

      await using var cmd = new NpgsqlCommand("SELECT id, key, pc_connected, mobile_connected FROM sessions WHERE key = @key", conn);
      cmd.Parameters.AddWithValue("key", key);

      await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
      if (await reader.ReadAsync(cancellationToken))
      {
        var session = new GameSession(reader.GetString(1));

        return session;
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
