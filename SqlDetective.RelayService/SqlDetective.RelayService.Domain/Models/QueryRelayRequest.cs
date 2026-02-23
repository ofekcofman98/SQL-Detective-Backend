namespace SqlDetective.RelayService.Domain.Models;

public record QueryRelayRequest
{
    public required string QueryString { get; init; }
}
