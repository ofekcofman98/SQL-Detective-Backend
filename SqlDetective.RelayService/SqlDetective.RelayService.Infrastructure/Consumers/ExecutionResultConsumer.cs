using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SqlDetective.RelayService.Domain.Models;
using SqlDetective.RelayService.Infrastructure.Hubs;
using SqlDetective.RelayService.Infrastructure.MessageQueue;

namespace SqlDetective.RelayService.Infrastructure.Consumers;

public class ExecutionResultConsumer : BackgroundService
{
    private readonly ILogger<ExecutionResultConsumer> _logger;
    private readonly IHubContext<GameRelayHub> _hubContext;
    private readonly RabbitMqConfiguration _config;
    private IConnection? _connection;
    private IChannel? _channel;

    public ExecutionResultConsumer(
        ILogger<ExecutionResultConsumer> logger,
        IHubContext<GameRelayHub> hubContext,
        IOptions<RabbitMqConfiguration> config)
    {
        _logger = logger;
        _hubContext = hubContext;
        _config = config.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[RelayService] Starting ExecutionResultConsumer...");

        await InitializeRabbitMqAsync(stoppingToken);

        _logger.LogInformation("[RelayService] ResultConsumer is ready and listening to queue: {Queue}",
            _config.ExecutionResultQueue);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task InitializeRabbitMqAsync(CancellationToken stoppingToken)
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

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: _config.ExecutionResultQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken
        );

        await _channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: stoppingToken
        );

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            await HandleResultAsync(ea, stoppingToken);
        };

        await _channel.BasicConsumeAsync(
            queue: _config.ExecutionResultQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken
        );

        _logger.LogInformation("[RelayService] RabbitMQ connection established to {Host}:{Port}",
            _config.HostName, _config.Port);
    }

    private async Task HandleResultAsync(BasicDeliverEventArgs ea, CancellationToken stoppingToken)
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);

        try
        {
            var result = JsonSerializer.Deserialize<ExecutionResult>(message);

            if (result == null)
            {
                _logger.LogError("[RelayService] Failed to deserialize result message: {Message}", message);
                await _channel!.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
                return;
            }

            _logger.LogInformation(
                "[RelayService] Received execution result. CorrelationId: {CorrelationId}, SessionKey: {SessionKey}, Success: {Success}",
                result.CorrelationId, result.SessionKey, result.Success);

            var connectionId = GameRelayHub.GetConnectionId(result.SessionKey);

            if (connectionId != null)
            {
                await _hubContext.Clients.Client(connectionId).SendAsync(
                    "QueryResult",
                    new
                    {
                        correlationId = result.CorrelationId,
                        sessionKey = result.SessionKey,
                        success = result.Success,
                        data = result.Data,
                        errorMessage = result.ErrorMessage,
                        timestamp = result.Timestamp
                    },
                    stoppingToken
                );

                _logger.LogInformation(
                    "[RelayService] Successfully pushed result to PC client via SignalR. CorrelationId: {CorrelationId}, ConnectionId: {ConnectionId}",
                    result.CorrelationId, connectionId);
            }
            else
            {
                _logger.LogWarning(
                    "[RelayService] No PC client connected for session: {SessionKey}. Result will be lost. CorrelationId: {CorrelationId}",
                    result.SessionKey, result.CorrelationId);
            }

            await _channel!.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "[RelayService] JSON deserialization error for result message: {Message}", message);
            await _channel!.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RelayService] Error processing result message: {Message}", message);
            await _channel!.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[RelayService] Stopping ResultConsumer...");

        if (_channel != null)
        {
            await _channel.CloseAsync(cancellationToken);
            _channel.Dispose();
        }

        if (_connection != null)
        {
            await _connection.CloseAsync(cancellationToken);
            _connection.Dispose();
        }

        await base.StopAsync(cancellationToken);
    }
}
