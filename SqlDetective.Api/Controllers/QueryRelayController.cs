using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlDetective.Domain.Query.Service;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using SqlDetective.Data.Postgres.Query;


namespace SqlDetective.Api.Controllers
{
    [Route("api/relay")]
    [ApiController]
    public class QueryRelayController : ControllerBase
    {
        private readonly IQueryRelayService r_QueryRelayService;
        private readonly IQueryExecutionService r_QueryExecutionService;
        private readonly ILogger<QueryRelayController> r_Logger;

        public QueryRelayController(IQueryRelayService i_QueryRelayService, ILogger<QueryRelayController> i_Logger, IQueryExecutionService i_QueryExecutionService)
        {
            r_QueryRelayService = i_QueryRelayService;
            r_QueryExecutionService = i_QueryExecutionService;
            r_Logger = i_Logger;
        }

        //[HttpPost(Name = "query")]
    [HttpPost]
    public async Task<IActionResult> SendQuery([FromQuery] string key, [FromBody] JsonElement queryBody, CancellationToken ct)
        {
            r_Logger.LogInformation("[QueryRelay] [POST] starting SendQuery");
            //r_Logger.LogInformation($"input: {queryJson}");

            if (string.IsNullOrEmpty(key))
            {
                return BadRequest("Missing Key");
            }

            //if (string.IsNullOrWhiteSpace(queryJson))
            //{
            //    return BadRequest("Missing query payload");
            //}
            
            string queryJson = queryBody.GetRawText();

            bool ok = await r_QueryRelayService.SaveIncomingQueryAsync(key, queryJson, ct);
          
            if (!ok)
            {
                return NotFound($"Session with key {key} was not found");
            }

            return Ok(new { message = "Query stored successfully" });
        }

        [HttpGet]
        public async Task<IActionResult> GetNextQuery([FromQuery] string key, CancellationToken ct)
        {
      //r_Logger.LogInformation("[QueryRelay] [GET] starting GetNextQuery");

      if (string.IsNullOrEmpty(key)) return BadRequest("Missing Key");

      string? queryJson = await r_QueryRelayService.GetNextQueryForPcAsync(key, ct);
      if (queryJson == null) return NoContent();

      // כאן הקסם: במקום להחזיר רק את ה-JSON הגולמי, 
      // אנחנו נריץ את ה-SQL ונצרף את התוצאות ל-JSON
      JObject queryObj = JObject.Parse(queryJson);
      string sql = queryObj["QueryString"]?.ToString() ?? "";

      if (!string.IsNullOrEmpty(sql))
      {
        JArray results = await r_QueryExecutionService.ExecuteAsync(key, sql, ct);
        queryObj["Results"] = results; // אנחנו מוסיפים שדה חדש ל-JSON הקיים
      }

      return Content(queryObj.ToString(), "application/json");
      //return Content(queryJson, "application/json");
    }
    }
}
