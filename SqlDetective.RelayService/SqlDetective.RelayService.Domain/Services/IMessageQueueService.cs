namespace SqlDetective.RelayService.Domain.Services;

public interface IMessageQueueService
{
    Task PublishExecutionRequestAsync<T>(string queueName, T message, CancellationToken ct = default) where T : class;
    Task<bool> IsHealthyAsync(CancellationToken ct = default);
}
