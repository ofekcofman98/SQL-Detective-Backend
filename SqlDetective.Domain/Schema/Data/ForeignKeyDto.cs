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
        public string FromTable { get; set; } = string.Empty;
        public string ToTable { get; set; } = string.Empty;
        public string FromColumn { get; set; } = string.Empty;
        public string ToColumn { get; set; } = string.Empty;

    }
}
