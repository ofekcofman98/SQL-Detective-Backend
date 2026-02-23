namespace SqlDetective.RelayService.Domain.Services;

public interface ISessionValidationService
{
    Task<bool> ValidateSessionAsync(string sessionKey, CancellationToken ct = default);
}
