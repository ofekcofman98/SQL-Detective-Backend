using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlDetective.Domain.Sessions.Data;

namespace SqlDetective.Domain.Sessions.Repository
{
    public interface ISessionRepository
    {
        Task<GameSession?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

        Task<GameSession> CreateAsync(GameSession session, CancellationToken cancellationToken = default);

        Task UpdateAsync(GameSession session, CancellationToken cancellationToken = default);
    }
}
