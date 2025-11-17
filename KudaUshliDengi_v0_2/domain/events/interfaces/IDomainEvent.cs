namespace KudaUshliDengi_v0_2.domain.events.interfaces;

public interface IDomainEvent
{
    DateTime OccuredOn { get; }
    Guid EventId { get; }
}