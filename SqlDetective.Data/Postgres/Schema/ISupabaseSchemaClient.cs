using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Data.Postgres.Schema
{
    public interface ISupabaseSchemaClient
    {
        Task<JArray> CallRpcAsync(string functionName, JObject body, CancellationToken ct = default);

    }
}
