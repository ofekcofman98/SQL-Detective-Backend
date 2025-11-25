using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Domain.Sessions.Generator
{
    public interface IKeyGenerator
    {
        string Generate();
    }
}
