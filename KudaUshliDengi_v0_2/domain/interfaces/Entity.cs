using KudaUshliDengi_v0_2.domain.events.interfaces;

namespace KudaUshliDengi_v0_2.domain.interfaces;
public abstract class Entity<T> where T : notnull, IEquatable<T>
{
    public T Id { get; protected set; }
    
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);
    protected void ClearDomainEvents() => _domainEvents.Clear();

    public IReadOnlyList<IDomainEvent> PopDomainEvents()
    {
        var events = _domainEvents.ToList();
        ClearDomainEvents();
        return events;
    }

    public override bool Equals(object? obj)
        => obj is Entity<T> other && Id.Equals(other.Id);

    public override int GetHashCode()
        => Id.GetHashCode();
    
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; protected set; }

    public void Delete() => DeletedAt = DateTime.UtcNow;
}