namespace KudaUshliDengi_v0_2.domain.events.interfaces;

public interface IDomainDispatcher
{
    Task DispatchAsync(IDomainEvent @event, CancellationToken cancellationToken = default);
    Task DispatchAsync(List<IDomainEvent> @events, CancellationToken cancellationToken = default);
}