using KudaUshliDengi_v0_2.domain.interfaces;

namespace KudaUshliDengi_v0_2.domain.valueobjects.ids;

public record CategoryId(Guid value) : EntityId<Guid>(value)
{
    public static CategoryId New() => new CategoryId(Guid.NewGuid());
    public static CategoryId Empty => new CategoryId(Guid.Empty);
}