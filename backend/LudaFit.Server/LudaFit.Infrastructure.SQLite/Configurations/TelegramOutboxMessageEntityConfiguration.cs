using LudaFit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LudaFit.Infrastructure.SQLite.Configurations;

public sealed class TelegramOutboxMessageEntityConfiguration : IEntityTypeConfiguration<TelegramOutboxMessage>
{
    public void Configure(EntityTypeBuilder<TelegramOutboxMessage> builder)
    {
        builder.ToTable("TelegramOutboxMessages");
        
        builder.HasKey(telegramMessage => telegramMessage.Id);

        ConfigureBasicProperties(builder);

        ConfigureBookingRelation(builder);
        
        ConfigureIndexes(builder);
    }

    private static void ConfigureBasicProperties(EntityTypeBuilder<TelegramOutboxMessage> builder)
    {
        builder.Property(telegramMessage => telegramMessage.AttemptsCount)
            .IsRequired();

        builder.Property(telegramMessage => telegramMessage.MaxAttemptNumber)
            .HasDefaultValue(5)
            .IsRequired();
        
        builder.Ignore(telegramMessage => telegramMessage.UsedAllAttempts);

        builder.Property(telegramMessage => telegramMessage.NextAttemptAtUtc)
            .IsRequired();
    }

    private static void ConfigureBookingRelation(EntityTypeBuilder<TelegramOutboxMessage> builder)
    {
        builder.HasOne<Booking>(telegramMessage => telegramMessage.Booking)
            .WithOne()
            .HasForeignKey<TelegramOutboxMessage>(telegramMessage => telegramMessage.BookingId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        builder.Navigation(telegramMessage => telegramMessage.Booking)
            .UsePropertyAccessMode(PropertyAccessMode.Property);
    }

    private static void ConfigureIndexes(EntityTypeBuilder<TelegramOutboxMessage> builder)
    {
        builder.HasIndex(telegramMessage => new { telegramMessage.AttemptsCount, telegramMessage.NextAttemptAtUtc });
    }
}
