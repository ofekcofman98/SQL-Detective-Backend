using SqlDetective.Domain.Schema.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Domain.Schema.Service
{
    public interface ISchemaService
    {
        Task<SchemaDto> LoadSchemaAsync(CancellationToken ct = default);
    }
}
