namespace SqlDetective.ExecutionEngine.Data.MessageQueue;

public record ExecutionRequest
{
    public required string CorrelationId { get; init; }
    public required string SessionKey { get; init; }
    public required string Sql { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
