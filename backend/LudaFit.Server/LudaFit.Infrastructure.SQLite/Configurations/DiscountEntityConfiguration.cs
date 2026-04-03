using LudaFit.Domain.Entities;
using LudaFit.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LudaFit.Infrastructure.SQLite.Configurations;

public sealed class DiscountEntityConfiguration : IEntityTypeConfiguration<Discount>
{
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        builder.ToTable("Discounts");
        
        builder.HasKey(discount => discount.Id);
        
        ConfigureBasicProperties(builder);
        
        ConfigureValueObjects(builder);
        
        ConfigureServiceRelation(builder);
    }

    private static void ConfigureBasicProperties(EntityTypeBuilder<Discount> builder)
    {
        builder.Property(discount => discount.Percent)
            .IsRequired();
        
        builder.Property(discount => discount.Status)
            .IsRequired()
            .HasConversion<string>();
    }

    private static void ConfigureValueObjects(EntityTypeBuilder<Discount> builder)
    {
        builder.ComplexProperty<DateRange>(discount => discount.DateRange, dateRange =>
        {
            dateRange.Property(dr => dr.Start)
                .IsRequired()
                .HasColumnName("StartDate");

            dateRange.Property(dr => dr.End)
                .IsRequired()
                .HasColumnName("EndDate");
        });
    }

    private static void ConfigureServiceRelation(EntityTypeBuilder<Discount> builder)
    {
        builder.HasMany<Service>("_services")
            .WithOne(service => service.Discount)
            .HasForeignKey(service => service.DiscountId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        
        builder.Navigation("_services")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
