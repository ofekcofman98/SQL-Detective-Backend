using SqlDetective.Domain.Query.Data;
using SqlDetective.Domain.Query.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Data.InMemory
{
    public class InMemoryRelayQueryRepository : IRelayQueryRepository
    {
        private readonly List<RelayQuery> _relayQueries = new();

        public Task<RelayQuery?> GetLatestPendingForSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
        {
            var relay = _relayQueries
                .Where(q => q.SessionId == sessionId && !q.IsConsumed)
                .OrderByDescending(q => q.CreatedAt)
                .FirstOrDefault();

            return Task.FromResult(relay);
        }

        public Task AddAsync(RelayQuery relayQuery, CancellationToken CancellationToken = default)
        {
            _relayQueries.Add(relayQuery);

            return Task.CompletedTask;
        }


        public Task UpdateAsync(RelayQuery relayQuery, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
