using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlDetective.Domain.Query.Service;

namespace SqlDetective.Api.Controllers
{
    [Route("api/relay")]
    [ApiController]
    public class QueryRelayController : ControllerBase
    {
        private readonly IQueryRelayService r_QueryRelayService;

        public QueryRelayController(QueryRelayService i_QueryRelayService)
        {
            r_QueryRelayService = i_QueryRelayService;
        }

        [HttpPost(Name = "query")]
        public async Task<IActionResult> SendQuery([FromQuery] string key, [FromBody] object queryBody, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(key))
            {
                return BadRequest("Missing Key");
            }

            if (queryBody == null)
            {
                return BadRequest("Missing query payload");
            }

            string queryJson = System.Text.Json.JsonSerializer.Serialize(queryBody);

            var ok = await r_QueryRelayService.SaveIncomingQueryAsync(key, queryJson, ct);
            if (!ok)
            {
                return NotFound($"Session with key {key} was not found");
            }

            return Ok(new { message = "Query stored successfully" });
        }

        [HttpGet("query")]
        public async Task<IActionResult> GetNextQuery([FromQuery] string key, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(key))
            {
                return BadRequest("Missing Key");
            }

            var queryJson = await r_QueryRelayService.GetNextQueryForPcAsync(key, ct);

            if (queryJson == null)
            {
                return NoContent();
            }

            return Content(queryJson, "application/json");
        }
    }
}
