using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlDetective.Domain.Sessions.Data;

namespace SqlDetective.Domain.Query.Data
{
    public class RelayQuery
    {
        public Guid Id { get; private set; }
        public Guid SessionId { get; private set; }
        public GameSession Session { get; private set; }

        public string QueryJson { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? ConsumedAt { get; private set; }
        public bool IsConsumed => ConsumedAt.HasValue;

        private RelayQuery() { }

        public RelayQuery(Guid i_SessionId, string i_QueryJson)
        {
            Id = Guid.NewGuid();
            SessionId = i_SessionId;
            QueryJson = i_QueryJson;
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkConsumed()
        {
            if (!IsConsumed)
            {
                ConsumedAt = DateTime.UtcNow;
            }
        }

    }
}
