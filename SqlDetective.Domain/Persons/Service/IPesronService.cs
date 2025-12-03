using SqlDetective.Domain.Persons.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Domain.Persons.Service
{
    public interface IPersonService
    {
        Task<IReadOnlyList<PersonDto>> GetAllAsync(CancellationToken ct = default);
    }
}
