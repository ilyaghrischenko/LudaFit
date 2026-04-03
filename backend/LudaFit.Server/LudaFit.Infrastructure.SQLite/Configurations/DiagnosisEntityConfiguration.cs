using LudaFit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LudaFit.Infrastructure.SQLite.Configurations;

public sealed class DiagnosisEntityConfiguration : IEntityTypeConfiguration<Diagnosis>
{
    public void Configure(EntityTypeBuilder<Diagnosis> builder)
    {
        ConfigureBasicProperties(builder);
        
        ConfigureBookingRelation(builder);
    }

    private static void ConfigureBasicProperties(EntityTypeBuilder<Diagnosis> builder)
    {
        builder.Property(diagnosis => diagnosis.Name)
            .IsRequired();
    }

    private static void ConfigureBookingRelation(EntityTypeBuilder<Diagnosis> builder)
    {
        builder.HasOne<Booking>(diagnosis => diagnosis.Booking)
            .WithMany("_diagnoses")
            .HasForeignKey(diagnosis => diagnosis.BookingId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        builder.Navigation(diagnosis => diagnosis.Booking)
            .UsePropertyAccessMode(PropertyAccessMode.Property);
    }
}
