using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Domain.Schema.Data
{
    [Serializable]
    public class SchemaDto
    {
        public List<TableDto> Tables { get; set; }
    }
}
