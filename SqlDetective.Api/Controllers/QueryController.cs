using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SqlDetective.Domain.Query.Data;
using SqlDetective.Domain.Query.Service;

namespace SqlDetective.Api.Controllers
{
    [ApiController]
    [Route("api/query")]
    public class QueryController : ControllerBase
    {
        private readonly IQueryExecutionService r_QueryExecutionService;
        private readonly ILogger<QueryController> r_Logger;

        public QueryController(IQueryExecutionService i_QueryExecutionService, ILogger<QueryController> logger)
        {
            r_QueryExecutionService = i_QueryExecutionService;
            r_Logger = logger;
        }

        [HttpPost("execute")]
        public async Task<ActionResult<JArray>> Execute([FromBody] ExecuteQueryRequest request, CancellationToken ct = default)
        {
            r_Logger.LogInformation("[Query] [POST] Execute, starting");

            if (string.IsNullOrWhiteSpace(request?.Key))
            {
                return BadRequest("Missing Key");
            }

            if (string.IsNullOrWhiteSpace(request.Sql))
            {
                return BadRequest("Missing Sql");
            }

            try
            {
                JArray rows = await r_QueryExecutionService.ExecuteAsync(request.Key, request.Sql, ct);
                return Ok(rows);
            }
            catch (InvalidOperationException ex)
            {
                r_Logger.LogWarning(ex, "Query execution failed");
                return NotFound(ex.Message);
            }
        }
    }
}
