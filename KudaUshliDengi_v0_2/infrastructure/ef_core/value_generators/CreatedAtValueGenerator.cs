using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace KudaUshliDengi_v0_2.infrastructure.ef_core.value_generators;

internal class CreatedAtValueGenerator : ValueGenerator<DateOnly>
{
    public override DateOnly Next(EntityEntry entry)
        => DateOnly.FromDateTime(DateTime.UtcNow);

    public override bool GeneratesTemporaryValues => false;
}