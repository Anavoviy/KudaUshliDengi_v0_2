using KudaUshliDengi_v0_2.domain.models;
using KudaUshliDengi_v0_2.domain.valueobjects;
using KudaUshliDengi_v0_2.domain.valueobjects.ids;
using KudaUshliDengi_v0_2.infrastructure.ef_core.value_generators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KudaUshliDengi_v0_2.infrastructure.ef_core.configurations;

public class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.ToTable("goals");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                goalId => goalId.value,
                value => new GoalId(value)
            ).ValueGeneratedNever();
        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasConversion(
                userId => userId.value,
                value => new UserId(value)
            ).IsRequired();
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(100);
        builder.Property(x => x.TargetAmount)
            .HasColumnName("target_amount")
            .HasConversion(
                targetAmount => targetAmount.Amount,
                value => (Money)value
            ).HasPrecision(19, 2)
            .IsRequired();
        builder.Property(x => x.CurrentAmount)
            .HasColumnName("current_amount")
            .HasConversion(
                targetAmount => (decimal)targetAmount,
                value => (Money)value
            ).HasPrecision(19, 2)
            .IsRequired();
        builder.Property(x => x.IsCompleted)
            .HasColumnName("is_completed");
        
        
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasValueGenerator<CreatedAtValueGenerator>()
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at");

        builder.HasOne(x => x.User)
            .WithMany();
    }
}