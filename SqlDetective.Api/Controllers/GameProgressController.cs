using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlDetective.Domain.Progress.Data;
using SqlDetective.Domain.Progress.Service;

namespace SqlDetective.Api.Controllers
{
    [ApiController]
    [Route("api/game-progress")]
    public class GameProgressController : ControllerBase
    {
        private readonly IGameProgressService r_GameProgressService;
        private readonly ILogger<GameProgressController> r_Logger;


        public GameProgressController(IGameProgressService i_GameProgressService, ILogger<GameProgressController> i_Logger)
        {
            r_GameProgressService = i_GameProgressService;
            r_Logger = i_Logger;
        }

        [HttpPost]
        public async Task<IActionResult> SaveGame([FromBody] GameProgressSaveRequest gameProgressSaveRequest, CancellationToken ct = default)
        {
            r_Logger.LogInformation("[GameProgress] [POST] saving game");

            if (gameProgressSaveRequest == null)
            {
                return BadRequest("Missing game progress payload");
            }

            await r_GameProgressService.SaveAsync(gameProgressSaveRequest, ct);

            return Ok(new { message = "Progress saved" });
        }

        [HttpGet]
        public async Task<IActionResult> LoadGame([FromQuery] string key, CancellationToken ct = default)
        {
            r_Logger.LogInformation("[GameProgress] [GET] loading game");

            if (string.IsNullOrWhiteSpace(key))
            {
                return BadRequest("Missing key");
            }

            GameProgress? gameProgress = await r_GameProgressService.LoadAsync(key, ct);

            if (gameProgress == null)
            {
                return NotFound("game progress not found");
            }

            var dto = new GameProgressDto
            {
                SequenceIndex = gameProgress.SequenceIndex,
                CurrentMissionIndex = gameProgress.MissionIndex,
                Lives = gameProgress.Lives
            };

            return Ok(dto);
        }
    }
}
