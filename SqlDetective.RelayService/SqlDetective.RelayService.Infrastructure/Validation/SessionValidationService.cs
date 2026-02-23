using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SqlDetective.RelayService.Domain.Services;

namespace SqlDetective.RelayService.Infrastructure.Validation;

public class SessionValidationService : ISessionValidationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SessionValidationService> _logger;
    private readonly string _monolithBaseUrl;

    public SessionValidationService(
        IHttpClientFactory httpClientFactory,
        ILogger<SessionValidationService> logger,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _monolithBaseUrl = configuration["MonolithApi:BaseUrl"] ?? "http://localhost:5000";
    }

    public async Task<bool> ValidateSessionAsync(string sessionKey, CancellationToken ct = default)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            
            var response = await httpClient.GetAsync(
                $"{_monolithBaseUrl}/api/session/{sessionKey}",
                ct
            );

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Session {SessionKey} validated successfully", sessionKey);
                return true;
            }

_logger.LogWarning("Session {SessionKey} validation failed with status code: {StatusCode}", 
                sessionKey, response.StatusCode);
            return false;        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while validating session {SessionKey}", sessionKey);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while validating session {SessionKey}", sessionKey);
            return false;
        }
    }
}
