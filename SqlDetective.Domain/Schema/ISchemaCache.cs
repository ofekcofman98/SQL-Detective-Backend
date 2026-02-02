using SqlDetective.Domain.Schema.Data;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Domain.Schema
{
  public interface ISchemaCache
  {
    SchemaDto? GetSchema();
    void StoreSchema(SchemaDto schema);
    void Clear();
  }
}
