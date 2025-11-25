using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlDetective.Domain.Query.Data;

namespace SqlDetective.Domain.Query.Repository
{
    public interface IRelayQueryRepository
    {
        Task AddAsync(RelayQuery relayQuery, CancellationToken CancellationToken = default);

        Task<RelayQuery?> GetLatestPendingForSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
        Task UpdateAsync(RelayQuery relayQuery, CancellationToken cancellationToken = default);
    }
}
