namespace SqlDetective.RelayService.Domain.Models;

public record ExecutionResult
{
    public required string CorrelationId { get; init; }
    public required string SessionKey { get; init; }
    public bool Success { get; init; }
    public List<Dictionary<string, object>>? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
