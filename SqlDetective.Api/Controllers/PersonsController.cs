using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlDetective.Domain.Persons.Data;
using SqlDetective.Domain.Persons.Service;

namespace SqlDetective.Api.Controllers
{
    [ApiController]
    [Route("api/persons")]
    public class PersonsController : ControllerBase
    {
        private readonly IPersonService r_PersonService;

        public PersonsController(IPersonService i_PersonService)
        {
            r_PersonService = i_PersonService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<PersonDto>>> GetAll(CancellationToken ct)
        {
            var persons = await r_PersonService.GetAllAsync(ct);
            return Ok(persons);
        }
    }
}
