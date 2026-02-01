using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Domain.Sessions
{
  public class InMemorySessionCache : ISessionCache
  {
    private readonly ConcurrentDictionary<string, Guid> _cache = new();

    public Guid? GetSessionId(string key)
        => _cache.TryGetValue(key, out var id) ? id : null;

    public void Store(string key, Guid sessionId)
        => _cache[key] = sessionId;
  }

}
