using Microsoft.AspNetCore.Mvc;
using SqlDetective.ExecutionEngine.Domain.Models;
using SqlDetective.ExecutionEngine.Domain.Services;

namespace SqlDetective.ExecutionEngine.Api.Controllers
{
    [ApiController]
    [Route("api/execution")]
    public class QueryExecutionController : ControllerBase
    {
        private readonly IQueryExecutionService r_QueryExecutionService;
        private readonly ILogger<QueryExecutionController> r_Logger;

        public QueryExecutionController(
            IQueryExecutionService queryExecutionService,
            ILogger<QueryExecutionController> logger)
        {
            r_QueryExecutionService = queryExecutionService;
            r_Logger = logger;
        }

        [HttpPost("execute")]
        public async Task<ActionResult<QueryExecutionResponse>> Execute(
            [FromBody] ExecuteQueryRequest request, 
            CancellationToken ct = default)
        {
            r_Logger.LogInformation("[ExecutionEngine] [POST] Execute starting");

            if (string.IsNullOrWhiteSpace(request?.SessionKey))
            {
                return BadRequest(new QueryExecutionResponse
                {
                    Success = false,
                    ErrorMessage = "Missing SessionKey"
                });
            }

            if (string.IsNullOrWhiteSpace(request.Sql))
            {
                return BadRequest(new QueryExecutionResponse
                {
                    Success = false,
                    ErrorMessage = "Missing SQL"
                });
            }

            var response = await r_QueryExecutionService.ExecuteAsync(
                request.SessionKey, 
                request.Sql, 
                ct);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", service = "execution-engine" });
        }
    }
}
