using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace SqlDetective.RelayService.Infrastructure.Hubs;

public class GameRelayHub : Hub
{
    private static readonly Dictionary<string, string> _sessionConnections = new();
    private static readonly object _lock = new();
    private readonly ILogger<GameRelayHub> _logger;

    public GameRelayHub(ILogger<GameRelayHub> logger)
    {
        _logger = logger;
    }

    public async Task RegisterSession(string sessionKey)
    {
        lock (_lock)
        {
            _sessionConnections[sessionKey] = Context.ConnectionId;
        }

        _logger.LogInformation(
            "[GameRelayHub] PC client registered. SessionKey: {SessionKey}, ConnectionId: {ConnectionId}",
            sessionKey, Context.ConnectionId);

        await Clients.Caller.SendAsync("SessionRegistered", new
        {
            sessionKey,
            connectionId = Context.ConnectionId,
            timestamp = DateTime.UtcNow
        });
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        lock (_lock)
        {
            var disconnectedSession = _sessionConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (disconnectedSession != null)
            {
                _sessionConnections.Remove(disconnectedSession);
                _logger.LogInformation(
                    "[GameRelayHub] PC client disconnected. SessionKey: {SessionKey}, ConnectionId: {ConnectionId}",
                    disconnectedSession, Context.ConnectionId);
            }
        }

        return base.OnDisconnectedAsync(exception);
    }

    public static string? GetConnectionId(string sessionKey)
    {
        lock (_lock)
        {
            return _sessionConnections.TryGetValue(sessionKey, out var connectionId) ? connectionId : null;
        }
    }

    public static bool IsSessionConnected(string sessionKey)
    {
        lock (_lock)
        {
            return _sessionConnections.ContainsKey(sessionKey);
        }
    }
}
