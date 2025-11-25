using SqlDetective.Domain.Sessions.Data;
using SqlDetective.Domain.Sessions.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Data.InMemory
{
    public class InMemorySessionRepository : ISessionRepository
    {
        private readonly Dictionary<string, GameSession> r_SessionRepository = new();
        public Task<GameSession?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            r_SessionRepository.TryGetValue(key, out var session);

            return Task.FromResult(session);
        }

        public Task<GameSession> CreateAsync(GameSession session, CancellationToken cancellationToken = default)
        {
            r_SessionRepository[session.Key] = session;

            return Task.FromResult(session);
        }


        public Task UpdateAsync(GameSession session, CancellationToken cancellationToken = default)
        {
            if (r_SessionRepository.ContainsKey(session.Key))
            {
                r_SessionRepository[session.Key] = session;
            }

            return Task.CompletedTask;
        }
    }
}
