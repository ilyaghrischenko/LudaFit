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
    }

    private static void ConfigureBasicProperties(EntityTypeBuilder<Booking> builder)
    {
        builder.Property(booking => booking.ServiceName)
            .IsRequired();
        
        builder.Property(booking => booking.ServicePrice)
            .IsRequired();

        builder.Property(booking => booking.FullName)
            .IsRequired();

        builder.Property(booking => booking.Purpose)
            .IsRequired();

        builder.Property(booking => booking.FoodWeighing)
            .IsRequired();
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

        builder.ComplexProperty<ClientHealth>(booking => booking.ClientHealth, healthQuestionnaire =>
        {
            healthQuestionnaire.Property(hq => hq.FeelingUnwellComplaints)
                .HasColumnName("ClientFeelingUnwellComplaints");

            healthQuestionnaire.Property(hq => hq.Allergies)
                .HasColumnName("ClientAllergies");

            healthQuestionnaire.Property(hq => hq.Intolerances)
                .HasColumnName("ClientIntolerances");

            healthQuestionnaire.Property(hq => hq.PhysicalActivities)
                .HasColumnName("ClientPhysicalActivities");

            healthQuestionnaire.Property(hq => hq.StressAndHowYouCopeWithIt)
                .HasColumnName("ClientStressAndHowYouCopeWithIt");

            healthQuestionnaire.Property(hq => hq.AnxietyTendency)
                .IsRequired()
                .HasColumnName("ClientAnxietyTendency");
        });
        
        builder.ComplexProperty<ClientFoodPreferences>(booking => booking.ClientFoodPreferences, clientFoodPreferences =>
        {
            clientFoodPreferences.Property(cfp => cfp.FavoriteFoods)
                .HasColumnName("ClientFavoriteFoods");
            
            clientFoodPreferences.Property(cfp => cfp.UnfavoriteFoods)
                .HasColumnName("ClientUnfavoriteFoods");
        });
    }

    private static void ConfigureDiagnosisRelation(EntityTypeBuilder<Booking> builder)
    {
        builder.HasMany<Diagnosis>("_diagnoses")
            .WithOne(diagnosis => diagnosis.Booking)
            .HasForeignKey(diagnosis => diagnosis.BookingId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        builder.Navigation("_diagnoses")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
