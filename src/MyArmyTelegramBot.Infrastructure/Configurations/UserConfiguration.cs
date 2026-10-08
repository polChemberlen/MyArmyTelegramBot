using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyArmyTelegramBot.Domain.Entities;

namespace MyArmyTelegramBot.Infrastructure.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.HasIndex(u => u.TelegramId)
                .IsUnique();

            builder.Property(u => u.TelegramId)
                .IsRequired();

            builder.Property(u => u.Username)
                .HasMaxLength(32);

            builder.Property(u => u.FirstName)
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(u => u.LastName)
                .HasMaxLength(64);

            builder.Property(u => u.Role)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(u => u.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(u => u.ApprovedAt);

            builder.Property(u => u.CreatedAt)
                .IsRequired();

            builder.Property(u => u.UpdatedAt);
        }
    }
}
