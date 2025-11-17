namespace KudaUshliDengi_v0_2.domain.events.interfaces;

public interface IBackgroundEventQueue
{
    void QueueEvent(IDomainEvent @event);
    Task<IDomainEvent> DequeueAsync(CancellationToken stoppingToken);
}