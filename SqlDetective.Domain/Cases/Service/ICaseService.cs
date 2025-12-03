using SqlDetective.Domain.Cases.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Domain.Cases.Service
{
    public interface ICaseService
    {
        Task<CaseDto> GetCaseAsync(string caseId, CancellationToken ct = default);
    }
}
