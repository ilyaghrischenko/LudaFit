using LudaFit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LudaFit.Infrastructure.SQLite.Configurations;

public sealed class EmailOutboxMessageEntityConfiguration : IEntityTypeConfiguration<EmailOutboxMessage>
{
    public void Configure(EntityTypeBuilder<EmailOutboxMessage> builder)
    {
        builder.ToTable("EmailOutboxMessages");
        
        builder.HasKey(email => email.Id);

        ConfigureBasicProperties(builder);

        ConfigureBookingRelation(builder);
        
        ConfigureIndexes(builder);
    }

    private static void ConfigureBasicProperties(EntityTypeBuilder<EmailOutboxMessage> builder)
    {
        builder.Property(email => email.AttemptNumber)
            .IsRequired();

        builder.Property(email => email.MaxAttemptNumber)
            .HasDefaultValue(5)
            .IsRequired();
        
        builder.Ignore(email => email.UsedAllAttempts);

        builder.Property(email => email.NextAttemptAt)
            .IsRequired();
    }

    private static void ConfigureBookingRelation(EntityTypeBuilder<EmailOutboxMessage> builder)
    {
        builder.HasOne<Booking>(email => email.Booking)
            .WithOne()
            .HasForeignKey<EmailOutboxMessage>(email => email.BookingId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        builder.Navigation(email => email.Booking)
            .UsePropertyAccessMode(PropertyAccessMode.Property);
    }

    private static void ConfigureIndexes(EntityTypeBuilder<EmailOutboxMessage> builder)
    {
        builder.HasIndex(email => new { email.AttemptNumber, email.NextAttemptAt });
    }
}
