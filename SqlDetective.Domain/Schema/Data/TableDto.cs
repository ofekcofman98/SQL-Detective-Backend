using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Domain.Schema.Data
{
    [Serializable]
    public class TableDto
    {
        public string Name { get; set; }
        public List<ColumnDto> Columns { get; set; }
        public List<ForeignKeyDto> ForeignKeys { get; set; }
    }
}
