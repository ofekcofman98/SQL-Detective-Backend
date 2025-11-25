using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlDetective.Domain.Sessions.Data;
using SqlDetective.Domain.Sessions.Generator;
using SqlDetective.Domain.Sessions.Repository;

namespace SqlDetective.Domain.Sessions.Service
{
    public class SessionService : ISessionService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IKeyGenerator _keyGenerator;

        public SessionService(ISessionRepository i_SessionRepository, IKeyGenerator i_KeyGenerator)
        {
            _sessionRepository = i_SessionRepository;
            _keyGenerator = i_KeyGenerator;
        }

        public async Task<GameSession> CreateSessionAsync(CancellationToken ct = default)
        {
            string key = _keyGenerator.Generate();

            GameSession session = new GameSession(key);

            return await _sessionRepository.CreateAsync(session, ct);
        }

        public async Task<GameSession?> GetGameSessionAsync(string i_Key, CancellationToken ct = default)
        {
            return await _sessionRepository.GetByKeyAsync(i_Key, ct);
        }
    }
}
