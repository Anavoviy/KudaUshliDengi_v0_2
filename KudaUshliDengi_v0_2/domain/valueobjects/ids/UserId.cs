using KudaUshliDengi_v0_2.domain.interfaces;

namespace KudaUshliDengi_v0_2.domain.valueobjects.ids;

public record UserId(Guid value) : EntityId<Guid>(value)
{
    public static UserId New() => new UserId(Guid.NewGuid());
    public static UserId Empty => new UserId(Guid.Empty);
} 