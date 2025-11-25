using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlDetective.Domain.Sessions.Service;

namespace SqlDetective.Api.Controllers
{
    [ApiController]
    [Route("api/session")]
    public class SessionController : ControllerBase
    {
        private readonly ISessionService r_SessionService;

        public SessionController(ISessionService i_SessionService)
        {
            r_SessionService = i_SessionService;
        }

        [HttpPost]
        public async Task<IActionResult> StartNewSession(CancellationToken ct = default)
        {
            var newSession = await r_SessionService.CreateSessionAsync(ct);

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
