using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Domain.Progress.Data
{
    public class GameProgressSaveRequest
    {
        public string Key { get; set; }
        public GameProgressDto Progress { get; set; }
    }
}
