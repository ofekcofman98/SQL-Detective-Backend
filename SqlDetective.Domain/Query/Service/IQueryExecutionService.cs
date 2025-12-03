using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SqlDetective.Domain.Query.Service
{
    public interface IQueryExecutionService
    {
        Task<JArray> ExecuteAsync(string sessionKey, string sql, CancellationToken ct = default);
    }
}
