using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlDetective.Domain.Schema.Data;
using SqlDetective.Domain.Schema.Service;

namespace SqlDetective.Api.Controllers
{
    [ApiController]
    [Route("api/schema")]
    public class SchemaController : ControllerBase
    {
        private readonly ISchemaService r_SchemaService;

        public SchemaController(ISchemaService i_SchemaService)
        {
            r_SchemaService = i_SchemaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSchema(CancellationToken ct = default)
        {
            SchemaDto schema = await r_SchemaService.LoadSchemaAsync(ct);

            if (schema == null)
            {
                return NotFound("No schema found");
            }

            return Ok(schema);
        }
    }
}
