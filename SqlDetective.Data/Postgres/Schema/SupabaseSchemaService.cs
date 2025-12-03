using Newtonsoft.Json.Linq;
using SqlDetective.Domain.Schema.Data;
using SqlDetective.Domain.Schema.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Data.Postgres.Schema
{
    public class SupabaseSchemaService : ISchemaService
    {
        private readonly ISupabaseSchemaClient r_Supabase;
        public SupabaseSchemaService(ISupabaseSchemaClient i_Supabase)
        {
            r_Supabase = i_Supabase;
        }

        public async Task<SchemaDto> LoadSchemaAsync(CancellationToken ct = default)
        {
            SchemaDto schema = new SchemaDto();
            Dictionary<string, TableDto> tablesByName = new Dictionary<string, TableDto>(StringComparer.OrdinalIgnoreCase);

            await LoadTablesAsync(schema, tablesByName, ct);
            await LoadColumnsAsync(schema, tablesByName, ct);
            await LoadForeignKeysAsync(schema, tablesByName, ct);

            return schema;
        }


        private async Task LoadTablesAsync(SchemaDto schema, Dictionary<string, TableDto> tablesByName, CancellationToken ct = default)
        {
            JArray tablesJson = await r_Supabase.CallRpcAsync("get_table_names", new JObject(), ct);

            foreach (JObject obj in tablesJson)
            {
                string tableName = obj["table_name"]!.ToString();

                var tableDto = new TableDto
                {
                    Name = tableName
                };

                schema.Tables.Add(tableDto);
                tablesByName[tableName] = tableDto;
            }
        }

        private async Task LoadColumnsAsync(SchemaDto schema, Dictionary<string, TableDto> tablesByName, CancellationToken ct = default)
        {
            foreach (TableDto table in schema.Tables)
            {
                var body = new JObject { ["table_name"] = table.Name };
                JArray columnsJson = await r_Supabase.CallRpcAsync("get_columns", body, ct);

                foreach (JObject col in columnsJson)
                {
                    table.Columns.Add(new ColumnDto
                    {
                        Name = col["column_name"]!.ToString(),
                        DataType = col["data_type"]!.ToString()
                    });
                }
            }
        }

        private async Task LoadForeignKeysAsync(SchemaDto schema, Dictionary<string, TableDto> tablesByName, CancellationToken ct = default)
        {
            JArray fksJson = await r_Supabase.CallRpcAsync("get_foreign_keys", new JObject(), ct);

            foreach (JObject fk in fksJson)
            {
                string fromTable = fk["table_name"]!.ToString();
                string fromColumn = fk["column_name"]!.ToString();
                string toTable = fk["foreign_table_name"]!.ToString();
                string toColumn = fk["foreign_column_name"]!.ToString();

                if (!tablesByName.TryGetValue(fromTable, out TableDto fromTableDto))
                {
                    continue;
                }

                fromTableDto.ForeignKeys.Add(new ForeignKeyDto
                {
                    FromTable = fromTable,
                    FromColumn = fromColumn,
                    ToTable = toTable,
                    ToColumn = toColumn
                });
            }
        }


    }
}
