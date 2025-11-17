using KudaUshliDengi_v0_2.domain.interfaces;

namespace KudaUshliDengi_v0_2.domain.valueobjects.ids;

public record OperationId(Guid value) : EntityId<Guid>(value)
{
    public static OperationId New() => new OperationId(Guid.NewGuid());
    public static OperationId Empty() => new OperationId(Guid.Empty);
}