using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlDetective.Domain.Progress.Data;

namespace SqlDetective.Domain.Progress.Repository
{
    public interface IGameProgressRepository
    {
        //Task<GameProgress?> LoadAsync(Guid sessionId, CancellationToken cancellationToken = default);
        Task<GameProgress?> LoadAsync(string key, CancellationToken cancellationToken = default);
        Task SaveAsync(GameProgress progress, CancellationToken cancellationToken = default);
    }
}
