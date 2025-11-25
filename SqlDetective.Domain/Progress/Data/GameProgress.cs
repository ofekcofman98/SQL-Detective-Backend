using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlDetective.Domain.Sessions.Data;

namespace SqlDetective.Domain.Progress.Data
{
    public class GameProgress
    {
        public Guid Id { get; private set; }
        public Guid SessionId { get; private set; }
        public GameSession Session { get; private set; }

        public int SequenceIndex { get; private set; }
        public int MissionIndex { get; private set; }
        public int Lives { get; private set; }

        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        private GameProgress() { }

        public GameProgress(Guid i_SessionId, int i_SequenceIndex, int i_MissionIndex, int i_Lives)
        {
            Id = Guid.NewGuid();
            SessionId = i_SessionId;
            SequenceIndex = i_SequenceIndex;
            MissionIndex = i_MissionIndex;
            Lives = i_Lives;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(int i_SequenceIndex, int i_MissionIndex, int i_Lives)
        {
            SequenceIndex = i_SequenceIndex;
            MissionIndex = i_MissionIndex;
            Lives = i_Lives;
            UpdatedAt = DateTime.UtcNow;
        }

    }
}
