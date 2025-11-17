using KudaUshliDengi_v0_2.domain.events.interfaces;

namespace KudaUshliDengi_v0_2.domain.events;

public abstract record DomainEvent : IDomainEvent
{
    public DateTime OccuredOn { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();

    protected DomainEvent()
    {
    }
}