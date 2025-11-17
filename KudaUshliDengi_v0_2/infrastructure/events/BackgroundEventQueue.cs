using System.Threading.Channels;
using KudaUshliDengi_v0_2.domain.events.interfaces;

namespace KudaUshliDengi_v0_2.infrastructure.events;

public class BackgroundEventQueue : IBackgroundEventQueue
{
    private readonly Channel<IDomainEvent> _channel = Channel.CreateUnbounded<IDomainEvent>();

    public void QueueEvent(IDomainEvent @event)
        => _channel.Writer.TryWrite(@event);

    public Task<IDomainEvent> DequeueAsync(CancellationToken stoppingToken)
        => _channel.Reader.ReadAsync(stoppingToken).AsTask();
}