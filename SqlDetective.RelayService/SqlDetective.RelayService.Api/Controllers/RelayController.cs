using Microsoft.AspNetCore.Mvc;
using SqlDetective.RelayService.Domain.Models;
using SqlDetective.RelayService.Domain.Services;

namespace SqlDetective.RelayService.Api.Controllers;

[ApiController]
[Route("api/relay")]
public class RelayController : ControllerBase
{
    private readonly IMessageQueueService _messageQueue;
    private readonly ISessionValidationService _sessionValidation;
    private readonly ILogger<RelayController> _logger;

    public RelayController(
        IMessageQueueService messageQueue,
        ISessionValidationService sessionValidation,
        ILogger<RelayController> logger)
    {
        _messageQueue = messageQueue;
        _sessionValidation = sessionValidation;
        _logger = logger;
    }

    [HttpPost("query")]
    public async Task<IActionResult> SendQuery(
        [FromQuery] string key,
        [FromBody] QueryRelayRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("[RelayService] Received query from mobile for session: {Key}", key);

        if (string.IsNullOrWhiteSpace(key))
        {
            return BadRequest(new { error = "Session key is required" });
        }

        if (string.IsNullOrWhiteSpace(request.QueryString))
        {
            return BadRequest(new { error = "QueryString is required" });
        }

        bool isValid = await _sessionValidation.ValidateSessionAsync(key, ct);
        if (!isValid)
        {
            _logger.LogWarning("[RelayService] Invalid session key: {Key}", key);
            return NotFound(new { error = $"Session '{key}' not found or inactive" });
        }

        var executionRequest = new ExecutionRequest
        {
            CorrelationId = Guid.NewGuid().ToString(),
            SessionKey = key,
            Sql = request.QueryString,
            Timestamp = DateTime.UtcNow
        };

        await _messageQueue.PublishExecutionRequestAsync(
            "sql-execution-requests",
            executionRequest,
            ct
        );

        _logger.LogInformation(
            "[RelayService] Published execution request to queue. CorrelationId: {CorrelationId}",
            executionRequest.CorrelationId
        );

        return Accepted(new
        {
            message = "Query accepted for processing",
            correlationId = executionRequest.CorrelationId,
            sessionKey = key
        });
    }

    [HttpGet("health")]
    public async Task<IActionResult> Health(CancellationToken ct)
    {
        var queueHealthy = await _messageQueue.IsHealthyAsync(ct);

        return Ok(new
        {
            service = "relay-service",
            status = queueHealthy ? "healthy" : "degraded",
            messageQueue = queueHealthy ? "connected" : "disconnected",
            timestamp = DateTime.UtcNow
        });
    }
}
