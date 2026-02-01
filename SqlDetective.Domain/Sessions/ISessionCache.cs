using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Domain.Sessions
{
  public interface ISessionCache
  {
    Guid? GetSessionId(string key);
    void Store(string key, Guid sessionId);
  }
}
