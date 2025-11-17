namespace KudaUshliDengi_v0_2.domain.interfaces;

public abstract record EntityId<TKey>(TKey value) where TKey : notnull, IEquatable<TKey>
{
    public override string ToString() => value.ToString();

    public virtual bool Equals(EntityId<TKey>? other)
        => other is not null && value.Equals(other.value);
    
    public override int GetHashCode()
        => value?.GetHashCode() ?? 0;
    
    public static implicit operator TKey(EntityId<TKey> id) 
        => id.value;
}