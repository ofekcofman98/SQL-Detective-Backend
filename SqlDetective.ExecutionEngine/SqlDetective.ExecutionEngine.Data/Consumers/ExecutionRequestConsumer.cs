using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SqlDetective.ExecutionEngine.Data.MessageQueue;
using SqlDetective.ExecutionEngine.Domain.Services;

namespace SqlDetective.ExecutionEngine.Data.Consumers;

public class ExecutionRequestConsumer : BackgroundService
{
    private readonly ILogger<ExecutionRequestConsumer> _logger;
    private readonly IQueryExecutionService _executionService;
    private readonly RabbitMqConfiguration _config;
    private IConnection? _connection;
    private IChannel? _channel;

    public ExecutionRequestConsumer(
        ILogger<ExecutionRequestConsumer> logger,
        IQueryExecutionService executionService,
        IOptions<RabbitMqConfiguration> config)
    {
        _logger = logger;
        _executionService = executionService;
        _config = config.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[ExecutionEngine] Starting RabbitMQ consumer...");

        await InitializeRabbitMqAsync(stoppingToken);

        _logger.LogInformation("[ExecutionEngine] Consumer is ready and listening to queue: {Queue}", 
            _config.ExecutionRequestQueue);

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
            queue: _config.ExecutionRequestQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken
        );

        await _channel.QueueDeclareAsync(
            queue: _config.ExecutionResultQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken
        );

        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            await HandleMessageAsync(ea, stoppingToken);
        };

        await _channel.BasicConsumeAsync(
            queue: _config.ExecutionRequestQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken
        );

        _logger.LogInformation("[ExecutionEngine] RabbitMQ connection established to {Host}:{Port}", 
            _config.HostName, _config.Port);
    }

    private async Task HandleMessageAsync(BasicDeliverEventArgs ea, CancellationToken stoppingToken)
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);

        try
        {
            var request = JsonSerializer.Deserialize<ExecutionRequest>(message);

            if (request == null)
            {
                _logger.LogError("[ExecutionEngine] Failed to deserialize message: {Message}", message);
                await _channel!.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
                return;
            }

            _logger.LogInformation(
                "[ExecutionEngine] Processing execution request. CorrelationId: {CorrelationId}, SessionKey: {SessionKey}",
                request.CorrelationId, request.SessionKey);

            var executionResponse = await _executionService.ExecuteAsync(
                request.SessionKey,
                request.Sql,
                stoppingToken
            );

            var result = new ExecutionResult
            {
                CorrelationId = request.CorrelationId,
                SessionKey = request.SessionKey,
                Success = executionResponse.Success,
                Data = executionResponse.Data,
                ErrorMessage = executionResponse.ErrorMessage,
                Timestamp = DateTime.UtcNow
            };

            await PublishResultAsync(result, stoppingToken);

            await _channel!.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);

            _logger.LogInformation(
                "[ExecutionEngine] Successfully processed request. CorrelationId: {CorrelationId}, Success: {Success}",
                request.CorrelationId, result.Success);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "[ExecutionEngine] JSON deserialization error for message: {Message}", message);
            await _channel!.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ExecutionEngine] Error processing message: {Message}", message);
            await _channel!.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
        }
    }

    private async Task PublishResultAsync(ExecutionResult result, CancellationToken stoppingToken)
    {
        var json = JsonSerializer.Serialize(result);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        await _channel!.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _config.ExecutionResultQueue,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: stoppingToken
        );

        _logger.LogInformation(
            "[ExecutionEngine] Published result to queue {Queue}. CorrelationId: {CorrelationId}",
            _config.ExecutionResultQueue, result.CorrelationId);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[ExecutionEngine] Stopping consumer...");

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
