using SqlDetective.Domain.Persons.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Domain.Persons
{
  public interface IPersonCache
  {
    IReadOnlyList<PersonDto>? GetPersons();
    void Store(IReadOnlyList<PersonDto> persons);
  }
}
