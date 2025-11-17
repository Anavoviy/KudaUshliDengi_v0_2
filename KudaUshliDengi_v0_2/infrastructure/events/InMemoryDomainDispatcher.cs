using KudaUshliDengi_v0_2.domain.events.interfaces;

namespace KudaUshliDengi_v0_2.infrastructure.events;

public class InMemoryDomainDispatcher(IBackgroundEventQueue eventQueue) : IDomainDispatcher
{
    public Task DispatchAsync(IDomainEvent @event, CancellationToken cancellationToken = default)
    {
        eventQueue.QueueEvent(@event);
        return Task.CompletedTask;
    }

    public Task DispatchAsync(List<IDomainEvent> @events, CancellationToken cancellationToken = default)
    {
        foreach (var @event in @events)
            eventQueue.QueueEvent(@event);
        return Task.CompletedTask;
    }
}