using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Domain.Query.Data
{
    public class ExecuteQueryRequest
    {
        public string Key { get; set; }
        public string Sql { get; set; }
    }
}
