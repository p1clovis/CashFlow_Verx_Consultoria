using System.Text;
using System.Text.Json;
using CashFlow.Transactions.Application.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace CashFlow.Transactions.Infrastructure.Messaging;

/// <summary>
/// Publishes domain events to RabbitMQ.
/// If the broker is unavailable the operation is logged and swallowed so
/// the Transaction service stays available (resilience requirement).
/// </summary>
public sealed class RabbitMqEventPublisher(
    IConnection connection,
    ILogger<RabbitMqEventPublisher> logger)
    : IEventPublisher, IDisposable
{
    private const string ExchangeName = "cashflow.events";
    private readonly IChannel _channel = connection.CreateChannelAsync().GetAwaiter().GetResult();

    public async Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : class
    {
        try
        {
            await _channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false,
                cancellationToken: ct);

            var body    = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));
            var props   = new BasicProperties { Persistent = true };
            var msgType = typeof(T).Name;

            await _channel.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: msgType,
                mandatory: false,
                basicProperties: props,
                body: body,
                cancellationToken: ct);

            logger.LogInformation("Published event {EventType} to exchange {Exchange}", msgType, ExchangeName);
        }
        catch (Exception ex)
        {
            // Swallow — transaction was already persisted; event will be replayed via outbox (future evolution)
            logger.LogError(ex, "Failed to publish event {EventType}. Transaction was persisted.", typeof(T).Name);
        }
    }

    public void Dispose() => _channel.Dispose();
}
