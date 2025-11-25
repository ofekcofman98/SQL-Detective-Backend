using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Domain.Progress.Data
{
    public class GameProgressDto
    {
        public int SequenceIndex { get; set; }
        public int CurrentMissionIndex { get; set; }
        public int Lives { get; set; }

    }
}
