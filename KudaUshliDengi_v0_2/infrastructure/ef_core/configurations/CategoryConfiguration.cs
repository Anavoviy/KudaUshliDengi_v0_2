using KudaUshliDengi_v0_2.domain.models;
using KudaUshliDengi_v0_2.domain.valueobjects.ids;
using KudaUshliDengi_v0_2.infrastructure.ef_core.value_generators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KudaUshliDengi_v0_2.infrastructure.ef_core.configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                categoryId => categoryId.value,
                value => new CategoryId(value)
            ).ValueGeneratedNever();
        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasConversion(
                userId => userId.value,
                value => new UserId(value)
            ).IsRequired();
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(x => x.ParentId)
            .HasColumnName("parent_id")
            .HasConversion(
                parentId => parentId != null ? parentId.value : (Guid?)null,
                value => value != null ? new CategoryId(value.Value) : null
            );
        
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasValueGenerator<CreatedAtValueGenerator>()
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at");
        
        builder.HasMany(x => x.Childrens)
            .WithOne(x => x.ParentCategory)
            .HasForeignKey(x => x.ParentId);
    }
}
