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
        private readonly ISessionCache _sessionCache;

    public SessionService(ISessionRepository i_SessionRepository, IKeyGenerator i_KeyGenerator, ISessionCache sessionCache)
        {
            _sessionRepository = i_SessionRepository;
            _keyGenerator = i_KeyGenerator;
      _sessionCache = sessionCache;
        }

        public async Task<GameSession> CreateSessionAsync(CancellationToken ct = default)
        {
          string key = _keyGenerator.Generate();

          Console.WriteLine($"key is {key}");

          GameSession session = new GameSession(key);

          var createdSession = await _sessionRepository.CreateAsync(session, ct);

          _sessionCache.Store(createdSession.Key, createdSession.Id);

          return createdSession;

        }

    public async Task<GameSession?> GetGameSessionAsync(string i_Key, CancellationToken ct = default)
        {
            return await _sessionRepository.GetByKeyAsync(i_Key, ct);
        }
    }
}
