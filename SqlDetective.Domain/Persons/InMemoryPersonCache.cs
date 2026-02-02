using SqlDetective.Domain.Persons.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Domain.Persons
{
  public class InMemoryPersonCache : IPersonCache
  {
    private IReadOnlyList<PersonDto>? _cachedPersons;

    public IReadOnlyList<PersonDto>? GetPersons() => _cachedPersons;

    public void Store(IReadOnlyList<PersonDto> persons) => _cachedPersons = persons;
  }
}
