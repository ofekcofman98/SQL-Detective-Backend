using SqlDetective.Domain.Schema.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Domain.Schema
{
  public class InMemorySchemaCache : ISchemaCache
  {
    private SchemaDto? _cachedSchema;

    public SchemaDto? GetSchema() => _cachedSchema;

    public void StoreSchema(SchemaDto schema) => _cachedSchema = schema;

    public void Clear() => _cachedSchema = null;
  }
}
