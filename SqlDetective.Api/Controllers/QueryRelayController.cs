using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlDetective.Domain.Query.Service;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using SqlDetective.Data.Postgres.Query;
using Newtonsoft.Json;       
using Newtonsoft.Json.Linq;  

namespace SqlDetective.Api.Controllers
{
    [Route("api/relay")]
    [ApiController]
    public class QueryRelayController : ControllerBase
    {
        private readonly IQueryRelayService r_QueryRelayService;
        private readonly IQueryExecutionService r_QueryExecutionService;
        private readonly ILogger<QueryRelayController> r_Logger;
    private static readonly SemaphoreSlim _queryExecutionSemaphore = new SemaphoreSlim(1, 1);

    public QueryRelayController(IQueryRelayService i_QueryRelayService, ILogger<QueryRelayController> i_Logger, IQueryExecutionService i_QueryExecutionService)
        {
            r_QueryRelayService = i_QueryRelayService;
            r_QueryExecutionService = i_QueryExecutionService;
            r_Logger = i_Logger;
        }

        //[HttpPost(Name = "query")]
        [HttpPost]
        public async Task<IActionResult> SendQuery([FromQuery] string key, [FromBody] JObject queryBody, CancellationToken ct)
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

      string queryJson = queryBody.ToString(Formatting.None);

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
      if (string.IsNullOrEmpty(key)) return BadRequest("Missing Key");

      string? queryJson = await r_QueryRelayService.GetNextQueryForPcAsync(key, ct);
      if (queryJson == null) return NoContent();

      return Content(queryJson, "application/json");
    }


    //  string? queryJson = await r_QueryRelayService.GetNextQueryForPcAsync(key, ct);
    //  if (queryJson == null) return NoContent();

    //  JObject queryObj = JObject.Parse(queryJson);
    //  string sql = queryObj["QueryString"]?.ToString() ?? "";

    //  if (!string.IsNullOrEmpty(sql))
    //  {
    //    await _queryExecutionSemaphore.WaitAsync(ct);
    //    try
    //    {
    //      // 1. קריאה לשירות שמחזיר עכשיו List<Dictionary<string, object>>
    //      var results = await r_QueryExecutionService.ExecuteAsync(key, sql, ct);

    //      // 2. המרה של הרשימה ל-JToken כדי ש-Newtonsoft יוכל להכניס אותה ל-queryObj
    //      // זה פותר את שגיאת הקומפילציה CS0029
    //      queryObj["Results"] = JToken.FromObject(results);
    //    }
    //    finally
    //    {
    //      _queryExecutionSemaphore.Release();
    //    }
    //  }

    //  return Content(queryObj.ToString(), "application/json");
    //}

    //    [HttpGet]
    //    public async Task<IActionResult> GetNextQuery([FromQuery] string key, CancellationToken ct)
    //    {

    //  if (string.IsNullOrEmpty(key)) return BadRequest("Missing Key");

    //  string? queryJson = await r_QueryRelayService.GetNextQueryForPcAsync(key, ct);
    //  if (queryJson == null) return NoContent();

    //  JObject queryObj = JObject.Parse(queryJson);
    //  string sql = queryObj["QueryString"]?.ToString() ?? "";

    //  if (!string.IsNullOrEmpty(sql))
    //  {
    //    await _queryExecutionSemaphore.WaitAsync(ct);
    //    try
    //    {
    //      JArray results = await r_QueryExecutionService.ExecuteAsync(key, sql, ct);
    //      queryObj["Results"] = results;
    //    }
    //    finally
    //    {
    //      _queryExecutionSemaphore.Release();
    //    }
    //  }

    //  return Content(queryObj.ToString(), "application/json");
    //  //return Content(queryJson, "application/json");
    //}
  }
}
