using SqlDetective.Domain.Progress.Data;
using SqlDetective.Domain.Progress.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Microsoft.Extensions.Logging;


namespace SqlDetective.Data.Postgres
{
    public class PostgresGameProgressRepository : IGameProgressRepository
    {
        private readonly string r_ConnectionString;
        private readonly ILogger<PostgresGameProgressRepository> r_Logger;

        public PostgresGameProgressRepository(
            string i_ConnectionString,
            ILogger<PostgresGameProgressRepository> i_Logger)
        {
            r_ConnectionString = i_ConnectionString;
            r_Logger = i_Logger;
        }

        public async Task<GameProgress?> LoadAsync(string key, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT id, key, sequence_index, mission_index, lives, updated_at
                FROM game_progress
                WHERE key = @key;";

            r_Logger.LogInformation("[DB] Opening connection with: {ConnStr}", r_ConnectionString);

            await using var conn = new NpgsqlConnection(r_ConnectionString);
            await conn.OpenAsync(cancellationToken);

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("key", key);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            Guid id = reader.GetGuid(reader.GetOrdinal("id"));
            //Guid sessionIdFromDb = reader.GetGuid(reader.GetOrdinal("session_id"));
            string keyFromDb = reader.GetString(reader.GetOrdinal("key"));
            int sequenceIndex = reader.GetInt32(reader.GetOrdinal("sequence_index"));
            int missionIndex = reader.GetInt32(reader.GetOrdinal("mission_index"));
            int lives = reader.GetInt32(reader.GetOrdinal("lives"));
            DateTime updatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at"));

            return GameProgress.CreateExisting(
            id,
            //sessionIdFromDb,
            keyFromDb,
            sequenceIndex,
            missionIndex,
            lives,
            updatedAt);
        }

        public async Task SaveAsync(GameProgress progress, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                INSERT INTO game_progress (id, key, sequence_index, mission_index, lives, updated_at)
                VALUES (@id, @key, @sequence_index, @mission_index, @lives, @updated_at)
                ON CONFLICT (key) DO UPDATE
                SET sequence_index = EXCLUDED.sequence_index,
                    mission_index  = EXCLUDED.mission_index,
                    lives          = EXCLUDED.lives,
                    updated_at     = EXCLUDED.updated_at;";

            await using var conn = new NpgsqlConnection(r_ConnectionString);
            await conn.OpenAsync(cancellationToken);

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", progress.Id);
            cmd.Parameters.AddWithValue("key", progress.Key);
            cmd.Parameters.AddWithValue("sequence_index", progress.SequenceIndex);
            cmd.Parameters.AddWithValue("mission_index", progress.MissionIndex);
            cmd.Parameters.AddWithValue("lives", progress.Lives);
            cmd.Parameters.AddWithValue("updated_at", progress.UpdatedAt);

            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
