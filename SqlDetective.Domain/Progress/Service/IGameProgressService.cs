using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlDetective.Domain.Progress.Data;

namespace SqlDetective.Domain.Progress.Service
{
    public interface IGameProgressService
    {
        Task SaveAsync(GameProgressSaveRequest gameProgressSaveRequest, CancellationToken ct = default);
        Task<GameProgress?> LoadAsync(string i_Key, CancellationToken ct = default);

    }
}
