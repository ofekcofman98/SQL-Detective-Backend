using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlDetective.Domain.Sessions.Service;
using Microsoft.Extensions.Logging;


namespace SqlDetective.Api.Controllers
{
    [ApiController]
    [Route("api/session")]
    public class SessionController : ControllerBase
    {
        private readonly ISessionService r_SessionService;
        private readonly ILogger<SessionController> r_Logger;


        public SessionController(ISessionService i_SessionService, ILogger<SessionController> i_Logger)
        {
            r_SessionService = i_SessionService;
            r_Logger = i_Logger;
        }

        [HttpPost]
        public async Task<IActionResult> StartNewSession(CancellationToken ct = default)
        {

            r_Logger.LogInformation("[Session] [POST] Starting StartNewSession");
            
            var newSession = await r_SessionService.CreateSessionAsync(ct);

            r_Logger.LogInformation("[Session] [POST] New session created. Key={Key}, Id={Id}", newSession.Key, newSession.Id);

            return Created(
                uri: $"/api/session/{newSession.Key}",
                value: new
                {
                    message = "Session created successfully",
                    key = newSession.Key,
                    sessionId = newSession.Id
                }
            );
        }

        [HttpGet(Name = "{key}")]
        public async Task<IActionResult> GetSession(string key, CancellationToken ct)
        {
            r_Logger.LogInformation("[Session] [GET] Starting GetSession");

            if (string.IsNullOrWhiteSpace(key))
            {
                return BadRequest("Missing Key");
            }

            var session = await r_SessionService.GetGameSessionAsync(key, ct);

            if (session == null)
            {
                return NotFound($"Session with key '{key}' not found");
            }

            return Ok(session);
        }



    }
}
