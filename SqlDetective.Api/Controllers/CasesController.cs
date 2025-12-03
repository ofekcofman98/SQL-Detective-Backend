using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlDetective.Domain.Cases.Data;
using SqlDetective.Domain.Cases.Service;

namespace SqlDetective.Api.Controllers
{
    [ApiController]
    [Route("api/cases")]
    public class CasesController : ControllerBase
    {
        private readonly ICaseService r_CaseService;

        public CasesController(ICaseService i_CaseService)
        {
            r_CaseService = i_CaseService;
        }

        [HttpGet("{caseId}")]
        public async Task<ActionResult<CaseDto>> GetCase(string caseId, CancellationToken ct)
        {
            CaseDto caseDto = await r_CaseService.GetCaseAsync(caseId, ct);
            if (caseDto == null)
            {
                return NotFound();
            }

            return Ok(caseDto);
        }


    }
}
