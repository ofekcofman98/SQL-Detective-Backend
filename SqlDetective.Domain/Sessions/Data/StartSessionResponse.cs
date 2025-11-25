using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Domain.Sessions.Data
{
    public class StartSessionResponse
    {
        public string Message { get; set; }
        public string Key { get; set; }
        public Guid SessionId { get; set; }

    }
}
