using SqlDetective.Domain.Progress.Data;
using SqlDetective.Domain.Progress.Repository;
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
        private readonly IGameProgressRepository r_ProgressRepository;

        public GameProgressService(ISessionRepository i_SessionRepository, IGameProgressRepository i_ProgressRepository)
        {
            r_SessionRepository = i_SessionRepository;
            r_ProgressRepository = i_ProgressRepository;
        }

        public async Task SaveAsync(GameProgressSaveRequest i_Request, CancellationToken ct = default)
        {
            GameSession? session = await r_SessionRepository.GetByKeyAsync(i_Request.Key);

            if (session == null)
            {
                throw new InvalidOperationException($"Session with key '{i_Request.Key}' not found");
            }

            //GameProgress gameProgress = new GameProgress(session.Id, i_Request.Progress.SequenceIndex, i_Request.Progress.CurrentMissionIndex, i_Request.Progress.Lives);

            //session.UpdateProgress(gameProgress);

            //await r_SessionRepository.UpdateAsync(session, ct);

            GameProgress? existing = await r_ProgressRepository.LoadAsync(i_Request.Key, ct);

            if (existing == null)
            {
                existing = new GameProgress(i_Request.Key,
                    i_Request.Progress.SequenceIndex,
                    i_Request.Progress.CurrentMissionIndex,
                    i_Request.Progress.Lives);
            }
            else
            {
                existing.Update(
                    i_Request.Progress.SequenceIndex,
                    i_Request.Progress.CurrentMissionIndex,
                    i_Request.Progress.Lives);
            }

            await r_ProgressRepository.SaveAsync(existing, ct);

            session.UpdateProgress(existing);
            await r_SessionRepository.UpdateAsync(session, ct);
        }

        public async Task<GameProgress?> LoadAsync(string i_Key, CancellationToken ct = default)
        {         
            return await r_ProgressRepository.LoadAsync(i_Key, ct);
        }
    }
}
