using SqlDetective.Domain.Progress.Data;
using SqlDetective.Domain.Sessions.Data;
using SqlDetective.Domain.Sessions.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Domain.Progress.Service
{
    public class GameProgressService : IGameProgressService
    {
        private readonly ISessionRepository r_SessionRepository;

        public GameProgressService(ISessionRepository i_SessionRepository)
        {
            r_SessionRepository = i_SessionRepository;
        }

        public async Task SaveAsync(GameProgressSaveRequest i_Request, CancellationToken ct = default)
        {
            GameSession? session = await r_SessionRepository.GetByKeyAsync(i_Request.Key);

            if (session == null)
            {
                throw new InvalidOperationException($"Session with key '{i_Request.Key}' not found");
            }

            GameProgress gameProgress = new GameProgress(session.Id, i_Request.Progress.SequenceIndex, i_Request.Progress.CurrentMissionIndex, i_Request.Progress.Lives);

            session.UpdateProgress(gameProgress);

            await r_SessionRepository.UpdateAsync(session, ct);
        }

        public async Task<GameProgress?> LoadAsync(string i_Key, CancellationToken ct = default)
        {
            var session = await r_SessionRepository.GetByKeyAsync(i_Key, ct);

            if (session == null)
            {
                return null;
            }

            return session.Progress;
        }
    }
}
