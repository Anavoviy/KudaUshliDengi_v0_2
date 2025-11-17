using KudaUshliDengi_v0_2.domain.interfaces;

namespace KudaUshliDengi_v0_2.domain.valueobjects.ids;

public record GoalId(Guid value) : EntityId<Guid>(value)
{
    public static GoalId New() => new GoalId(Guid.NewGuid());
    public static GoalId Empty() => new GoalId(Guid.Empty);
}