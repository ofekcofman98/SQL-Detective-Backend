using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlDetective.Domain.Sessions.Data;

namespace SqlDetective.Domain.Sessions.Service
{
    public interface ISessionService
    {
        Task<GameSession> CreateSessionAsync(/*string i_Key,*/ CancellationToken ct = default);
        Task<GameSession?> GetGameSessionAsync(string i_Key, CancellationToken ct = default);

    }
}
