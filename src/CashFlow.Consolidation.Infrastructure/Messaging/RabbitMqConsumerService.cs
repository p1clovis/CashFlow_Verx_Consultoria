using System.Text;
using System.Text.Json;
using CashFlow.Consolidation.Application.Events;
using CashFlow.Shared.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CashFlow.Consolidation.Infrastructure.Messaging;

/// <summary>
/// Long-running background service that consumes TransactionCreatedEvents
/// from RabbitMQ and delegates to the application handler.
/// Uses a dedicated queue bound to the fanout exchange.
/// </summary>
public sealed class RabbitMqConsumerService(
    IConnection connection,
    IServiceScopeFactory scopeFactory,
    ILogger<RabbitMqConsumerService> logger)
    : BackgroundService
{
    private const string ExchangeName = "cashflow.events";
    private const string QueueName    = "cashflow.consolidation";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Fanout, durable: true, cancellationToken: stoppingToken);
        await channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await channel.QueueBindAsync(QueueName, ExchangeName, routingKey: string.Empty, cancellationToken: stoppingToken);

        // Prefetch limit — prevents overwhelming this consumer on peak (50 req/s target)
        await channel.BasicQosAsync(0, prefetchCount: 10, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json  = Encoding.UTF8.GetString(ea.Body.ToArray());
                var @event = JsonSerializer.Deserialize<TransactionCreatedEvent>(json);

                if (@event is not null)
                {
                    using var scope   = scopeFactory.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<TransactionCreatedEventHandler>();
                    await handler.HandleAsync(@event, stoppingToken);
                }

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing message {DeliveryTag}", ea.DeliveryTag);
                // Nack without requeue — dead-letter queue should handle poison messages
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(QueueName, autoAck: false, consumer, stoppingToken);
        logger.LogInformation("Consolidation consumer started on queue '{Queue}'", QueueName);

        // Keep alive until cancellation
        await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
    }
}
