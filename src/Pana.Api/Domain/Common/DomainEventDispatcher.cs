using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Pana.Api.Domain.Common;

/// <summary>
/// In-process domain event dispatcher using Channel<T>.
/// Publishing is fire-and-forget. Processing is sequential per event type.
/// Replace Channel<T> with Kafka/RabbitMQ later if distributed messaging is needed.
/// </summary>
public class DomainEventDispatcher
{
    private readonly Channel<IDomainEvent> _channel;

    public DomainEventDispatcher()
    {
        _channel = Channel.CreateUnbounded<IDomainEvent>(new UnboundedChannelOptions
        {
            SingleReader = false, // Multiple handlers can read
            SingleWriter = false  // Multiple publishers
        });
    }

    public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default)
    {
        await _channel.Writer.WriteAsync(domainEvent, ct);
    }

    public IAsyncEnumerable<IDomainEvent> ReadAllAsync(CancellationToken ct)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }
}

/// <summary>
/// Background worker that reads domain events from the channel
/// and dispatches them to all registered handlers via DI scope.
/// </summary>
public class DomainEventBackgroundWorker : BackgroundService
{
    private readonly DomainEventDispatcher _dispatcher;
    private readonly IServiceScopeFactory _scopeFactory;

    public DomainEventBackgroundWorker(DomainEventDispatcher dispatcher, IServiceScopeFactory scopeFactory)
    {
        _dispatcher = dispatcher;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var domainEvent in _dispatcher.ReadAllAsync(stoppingToken))
        {
            try
            {
                await DispatchAsync(domainEvent, stoppingToken);
            }
            catch (Exception ex)
            {
                // Log and continue — don't lose the worker
                var logger = _scopeFactory.CreateScope()
                    .ServiceProvider.GetRequiredService<ILogger<DomainEventBackgroundWorker>>();
                logger.LogError(ex, "Error handling domain event {EventType}", domainEvent.GetType().Name);
            }
        }
    }

    private async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();

        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
        var handlers = scope.ServiceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            var method = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))
                ?? throw new InvalidOperationException($"Handler {handler!.GetType().Name} does not implement HandleAsync.");

            var task = (Task)method.Invoke(handler, [domainEvent, ct])!;
            await task;
        }
    }
}
