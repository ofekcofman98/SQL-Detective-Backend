using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Domain.Sessions.Generator
{
    public class RandomKeyGenerator : IKeyGenerator
    {
        private static readonly Random Rnd = new Random();
        public string Generate()
        {
            return Rnd.Next(100000, 999999).ToString();
        }
    }
}
