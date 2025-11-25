using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlDetective.Domain.Progress.Data;

namespace SqlDetective.Domain.Sessions.Data
{
    public class GameSession
    {
        public Guid Id { get; private set; }
        public string Key { get; private set; } = string.Empty;
        public bool PcConnected { get; private set; }
        public bool MobileConnected { get; private set; }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public GameProgress? Progress { get; private set; }
        private GameSession() { }
        public GameSession(string i_Key)
        {
            Id = Guid.NewGuid();
            Key = i_Key;
            PcConnected = true;
            MobileConnected = false;
        }

        public void ConnectMobile() { MobileConnected = true; }
        public void DisconnectMobile() { MobileConnected = false; }

        public void UpdateProgress(GameProgress i_GameProgress)
        {
            if (Progress == null)
            {
                Progress = i_GameProgress;
            }
            else
            {
                Progress.Update(i_GameProgress.SequenceIndex, i_GameProgress.MissionIndex, i_GameProgress.Lives);
            }
        }

    }
}
