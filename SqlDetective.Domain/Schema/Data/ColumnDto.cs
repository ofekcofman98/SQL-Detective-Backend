using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Domain.Schema.Data
{
    [Serializable]
    public class ColumnDto
    {
        public string Name { get; set; }
        public string DataType { get; set; }
    }
}
