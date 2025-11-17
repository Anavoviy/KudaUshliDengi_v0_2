using KudaUshliDengi_v0_2.domain.models;
using KudaUshliDengi_v0_2.domain.valueobjects;
using KudaUshliDengi_v0_2.domain.valueobjects.ids;
using KudaUshliDengi_v0_2.infrastructure.ef_core.value_generators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KudaUshliDengi_v0_2.infrastructure.ef_core.configurations;

public class OperationConfiguration : IEntityTypeConfiguration<Operation>
{
    public void Configure(EntityTypeBuilder<Operation> builder)
    {
        builder.ToTable("operations");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                operationId => operationId.value,
                value => new OperationId(value)
            ).ValueGeneratedNever();
        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasConversion(
                userId => userId.value,
                value => new UserId(value)
            ).ValueGeneratedNever();
        builder.Property(x => x.Amount)
            .HasColumnName("amount")
            .HasConversion(
                amount => amount.Amount,
                value => (Money)value)
            .ValueGeneratedNever();
        builder.Property(x => x.Type)
            .HasColumnName("type");
        builder.Property(x => x.Date)
            .HasColumnName("date")
            .HasConversion(
                date => date.ToString(),
                value => DateOnly.Parse(value)
            );
        
        builder.Property(x => x.CategoryId)
            .HasColumnName("category_id")
            .HasConversion(
                categoryId => categoryId != null ? categoryId.value : (Guid?)null,
                value => value != null ? new CategoryId(value.Value) : null
            );
        builder.Property(x => x.ParentId)
            .HasColumnName("parent_id")
            .HasConversion(
                parentId => parentId != null ? parentId.value : (Guid?)null,
                value => value != null ? new OperationId(value.Value) : null
            );
        
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasValueGenerator<CreatedAtValueGenerator>()
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at");
        
        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId);
    }
}