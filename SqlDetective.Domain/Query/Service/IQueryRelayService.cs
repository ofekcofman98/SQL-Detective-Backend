using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Domain.Query.Service
{
    public interface IQueryRelayService
    {
        Task<bool> SaveIncomingQueryAsync(string sessionKey, string queryJson, CancellationToken ct = default);
        Task<string?> GetNextQueryForPcAsync(string sessionKey, CancellationToken ct = default);


    }
}
