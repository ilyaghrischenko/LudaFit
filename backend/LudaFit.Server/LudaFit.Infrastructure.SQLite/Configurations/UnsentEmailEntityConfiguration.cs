using LudaFit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LudaFit.Infrastructure.SQLite.Configurations;

public sealed class UnsentEmailEntityConfiguration : IEntityTypeConfiguration<UnsentEmail>
{
    public void Configure(EntityTypeBuilder<UnsentEmail> builder)
    {
        builder.ToTable("UnsentEmails");
        
        builder.HasKey(unsentEmail => unsentEmail.Id);

        ConfigureBasicProperties(builder);

        ConfigureBookingRelation(builder);
        
        ConfigureIndexes(builder);
    }

    private static void ConfigureBasicProperties(EntityTypeBuilder<UnsentEmail> builder)
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

    private static void ConfigureBookingRelation(EntityTypeBuilder<UnsentEmail> builder)
    {
        builder.HasOne<Booking>(email => email.Booking)
            .WithOne()
            .HasForeignKey<UnsentEmail>(email => email.BookingId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        builder.Navigation(email => email.Booking)
            .UsePropertyAccessMode(PropertyAccessMode.Property);
    }

    private static void ConfigureIndexes(EntityTypeBuilder<UnsentEmail> builder)
    {
        builder.HasIndex(email => new { email.AttemptNumber, email.NextAttemptAt });
    }
}
