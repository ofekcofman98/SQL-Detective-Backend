using SqlDetective.Domain.Query.Data;
using SqlDetective.Domain.Query.Repository;
using SqlDetective.Domain.Sessions.Data;
using SqlDetective.Domain.Sessions.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Domain.Query.Service
{
    public class QueryRelayService : IQueryRelayService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IRelayQueryRepository _relayQueryRepository;

        public QueryRelayService(ISessionRepository i_SessionRepository, IRelayQueryRepository i_RelayQueryRepository)
        {
            _sessionRepository = i_SessionRepository;
            _relayQueryRepository = i_RelayQueryRepository;
        }

        public async Task<bool> SaveIncomingQueryAsync(string sessionKey, string queryJson, CancellationToken ct = default)
        {
            GameSession? session = await _sessionRepository.GetByKeyAsync(sessionKey);

            if (session == null)
            {
                Console.WriteLine("[SaveIncomingQueryAsync] session == null");
                return false;
            }

            var relay = new RelayQuery(session.Id, queryJson);
            await _relayQueryRepository.AddAsync(relay, ct);

            return true;
        }


        public async Task<string?> GetNextQueryForPcAsync(string sessionKey, CancellationToken ct = default)
        {
            var session = await _sessionRepository.GetByKeyAsync(sessionKey, ct);
            if (session == null)
            {
                return null;
            }

            var relay = await _relayQueryRepository.GetLatestPendingForSessionAsync(session.Id, ct);
            if (relay == null)
            {
                return null;
            }

            relay.MarkConsumed();
            await _relayQueryRepository.UpdateAsync(relay, ct);

            return relay.QueryJson;
        }
    }

}
