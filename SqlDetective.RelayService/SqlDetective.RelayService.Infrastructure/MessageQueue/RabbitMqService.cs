using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SqlDetective.RelayService.Domain.Services;

namespace SqlDetective.RelayService.Infrastructure.MessageQueue;

public class RabbitMqService : IMessageQueueService, IDisposable
{
    private readonly RabbitMqConfiguration _config;
    private readonly ILogger<RabbitMqService> _logger;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly object _lock = new();

    public RabbitMqService(IOptions<RabbitMqConfiguration> config, ILogger<RabbitMqService> logger)
    {
        _config = config.Value;
        _logger = logger;
        InitializeConnectionAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeConnectionAsync()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _config.HostName,
                Port = _config.Port,
                UserName = _config.UserName,
                Password = _config.Password,
                VirtualHost = _config.VirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(
                queue: _config.ExecutionRequestQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            await _channel.QueueDeclareAsync(
                queue: _config.ExecutionResultQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            _logger.LogInformation("RabbitMQ connection established to {Host}:{Port}", 
                _config.HostName, _config.Port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
            throw;
        }
    }

    public async Task PublishExecutionRequestAsync<T>(string queueName, T message, CancellationToken ct = default) where T : class
    {
        if (_channel == null || _channel.IsClosed)
        {
            _logger.LogWarning("Channel is closed, attempting to reconnect");
            await InitializeConnectionAsync();
        }

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        await _channel!.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: queueName,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: ct
        );

        _logger.LogInformation("Published message to queue {Queue}: {Message}", queueName, json);
    }

    public Task<bool> IsHealthyAsync(CancellationToken ct = default)
    {
        return Task.FromResult(_connection?.IsOpen == true && !(_channel?.IsClosed ?? true));
    }

    public void Dispose()
    {
        _channel?.CloseAsync().GetAwaiter().GetResult();
        _channel?.Dispose();
        _connection?.CloseAsync().GetAwaiter().GetResult();
        _connection?.Dispose();
    }
}
