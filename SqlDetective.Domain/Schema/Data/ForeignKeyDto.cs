using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Domain.Schema.Data
{
    [Serializable]
    public class ForeignKeyDto
    {
        public string FromTable { get; set; }
        public string ToTable { get; set; }
        public string FromColumn { get; set; }
        public string ToColumn { get; set; }

    }
}
