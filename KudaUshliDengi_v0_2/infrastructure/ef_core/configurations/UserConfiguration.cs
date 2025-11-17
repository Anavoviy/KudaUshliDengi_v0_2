using KudaUshliDengi_v0_2.domain.models;
using KudaUshliDengi_v0_2.domain.valueobjects.ids;
using KudaUshliDengi_v0_2.infrastructure.ef_core.value_generators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KudaUshliDengi_v0_2.infrastructure.ef_core.configurations;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                userId => userId.value,
                value => new UserId(value)
            ).ValueGeneratedNever();
        builder.Property(x => x.TgUserId)
            .HasColumnName("tg_user_id")
            .IsRequired();
        builder.Property(x => x.TgChatId)
            .HasColumnName("tg_chat_id")
            .IsRequired();
        builder.Property(x => x.TgUsername)
            .HasColumnName("tg_username")
            .HasMaxLength(100);
        
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasValueGenerator<CreatedAtValueGenerator>()
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at");

        builder.HasMany(x => x.Categories)
            .WithOne()
            .HasForeignKey(x => x.UserId);
    }
}