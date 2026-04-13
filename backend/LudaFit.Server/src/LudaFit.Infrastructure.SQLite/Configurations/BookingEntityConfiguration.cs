using System.Net.Mail;
using LudaFit.Domain.Entities;
using LudaFit.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LudaFit.Infrastructure.SQLite.Configurations;

public sealed class BookingEntityConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");
        
        builder.HasKey(booking => booking.Id);
        
        ConfigureBasicProperties(builder);
        
        ConfigureValueObjects(builder);
        
        ConfigureDiagnosisRelation(builder);
        
        ConfigureIndexes(builder);
    }

    private static void ConfigureBasicProperties(EntityTypeBuilder<Booking> builder)
    {
        builder.Property(booking => booking.IdempotencyKey)
            .IsRequired();
        
        builder.Property(booking => booking.ServiceName)
            .IsRequired();
        
        builder.Property(booking => booking.ServicePrice)
            .IsRequired();

        builder.Property(booking => booking.FullName)
            .IsRequired();

        builder.Property(booking => booking.Purpose)
            .IsRequired();

        builder.Property(booking => booking.PhysicalActivities)
            .IsRequired(false);
    }

    private static void ConfigureValueObjects(EntityTypeBuilder<Booking> builder)
    {
        builder.ComplexProperty<ClientMetrics>(booking => booking.ClientMetrics, clientMetrics =>
        {
            clientMetrics.Property(cm => cm.Age)
                .IsRequired()
                .HasColumnName("ClientAge");

            clientMetrics.Property(cm => cm.Height)
                .IsRequired()
                .HasColumnName("ClientHeight");

            clientMetrics.Property(cm => cm.Weight)
                .IsRequired()
                .HasColumnName("ClientWeight");

            clientMetrics.Property(cm => cm.WaistSize)
                .IsRequired()
                .HasColumnName("ClientWaistSize");
        });

        builder.ComplexProperty<ClientContacts>(booking => booking.ClientContacts, clientContacts =>
        {
            clientContacts.Property(cc => cc.Email)
                .IsRequired()
                .HasConversion<string>(email => email.ToString(), emailString => new MailAddress(emailString))
                .HasColumnName("ClientEmail");
            
            clientContacts.Property(cc => cc.PhoneNumber)
                .IsRequired()
                .HasColumnName("ClientPhoneNumber");
            
            clientContacts.Property(cc => cc.TelegramTag)
                .IsRequired(false)
                .HasColumnName("ClientTelegramTag");
        });
        
        builder.OwnsOne<ClientAdditionalInformation>(booking => booking.ClientAdditionalInformation, clientAdditionalInformation =>
        {
            clientAdditionalInformation.OwnsOne<ClientHealth>(cai => cai.ClientHealth, clientHealth =>
            {
                clientHealth.Property(hq => hq.FeelingUnwellComplaints)
                    .HasColumnName("ClientFeelingUnwellComplaints");

                clientHealth.Property(hq => hq.Allergies)
                    .HasColumnName("ClientAllergies");

                clientHealth.Property(hq => hq.Intolerances)
                    .HasColumnName("ClientIntolerances");

                clientHealth.Property(hq => hq.StressAndHowYouCopeWithIt)
                    .HasColumnName("ClientStressAndHowYouCopeWithIt");

                clientHealth.Property(hq => hq.AnxietyTendency)
                    .IsRequired()
                    .HasColumnName("ClientAnxietyTendency");
            });
        
            clientAdditionalInformation.OwnsOne<ClientFoodPreferences>(cai => cai.ClientFoodPreferences, clientFoodPreferences =>
            {
                clientFoodPreferences.Property(cfp => cfp.FavoriteFoods)
                    .HasColumnName("ClientFavoriteFoods");
            
                clientFoodPreferences.Property(cfp => cfp.UnfavoriteFoods)
                    .HasColumnName("ClientUnfavoriteFoods");
            });
            
            clientAdditionalInformation.Property(cai => cai.FoodWeighing)
                .HasColumnName("FoodWeighing")
                .IsRequired();
        });
    }

    private static void ConfigureDiagnosisRelation(EntityTypeBuilder<Booking> builder)
    {
        builder.HasMany<Diagnosis>("_diagnoses")
            .WithOne(diagnosis => diagnosis.Booking)
            .HasForeignKey(diagnosis => diagnosis.BookingId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        builder.Ignore(booking => booking.Diagnoses);
        
        builder.Navigation("_diagnoses")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureIndexes(EntityTypeBuilder<Booking> builder)
    {
        builder.HasIndex(booking => booking.IdempotencyKey)
            .IsUnique();
    }
}
