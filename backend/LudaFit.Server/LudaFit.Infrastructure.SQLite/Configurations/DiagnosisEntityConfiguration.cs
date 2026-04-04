using LudaFit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LudaFit.Infrastructure.SQLite.Configurations;

public sealed class DiagnosisEntityConfiguration : IEntityTypeConfiguration<Diagnosis>
{
    public void Configure(EntityTypeBuilder<Diagnosis> builder)
    {
        builder.ToTable("Diagnoses");
        
        builder.HasKey(diagnosis => diagnosis.Id);
        
        ConfigureBasicProperties(builder);
        
        ConfigureBookingRelation(builder);
        
        ConfigureMedicineRelation(builder);
        
        ConfigureIndexes(builder);
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

    private static void ConfigureMedicineRelation(EntityTypeBuilder<Diagnosis> builder)
    {
        builder.HasMany<Medicine>("_medicines")
            .WithOne(medicine => medicine.Diagnosis)
            .HasForeignKey(medicine => medicine.DiagnosisId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        builder.Ignore(diagnosis => diagnosis.Medicines);
        
        builder.Navigation("_medicines")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureIndexes(EntityTypeBuilder<Diagnosis> builder)
    {
        builder.HasIndex(diagnosis => diagnosis.BookingId);
        
        builder.HasIndex(diagnosis => diagnosis.Name)
            .IsUnique();
    }
}
