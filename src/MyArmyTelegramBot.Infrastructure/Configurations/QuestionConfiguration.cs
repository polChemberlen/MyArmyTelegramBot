using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyArmyTelegramBot.Domain.Entities;

namespace MyArmyTelegramBot.Infrastructure.Configurations
{
    public class QuestionConfiguration : IEntityTypeConfiguration<Question>
    {
        public void Configure(EntityTypeBuilder<Question> builder)
        {
            builder.ToTable("Questions");

            builder.HasKey(q => q.Id);

            builder.HasOne(q => q.User)
                .WithMany(u => u.Questions)
                .HasForeignKey(q => q.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(q => q.UserId)
                .IsRequired();

            builder.Property(q => q.QuestionText)
                .IsRequired()
                .HasMaxLength(4096);

            builder.Property(q => q.CreatedAt)
                .IsRequired();

            builder.Property(q => q.UpdatedAt);
        }
    }
}
