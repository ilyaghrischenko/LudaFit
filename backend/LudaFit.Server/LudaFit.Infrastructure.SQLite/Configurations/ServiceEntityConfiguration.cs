using LudaFit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LudaFit.Infrastructure.SQLite.Configurations;

public sealed class ServiceEntityConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("Services");

        builder.HasKey(service => service.Id);
        
        ConfigureBasicProperties(builder);
        
        ConfigureDiscountRelation(builder);
        
        ConfigureIndexes(builder);
    }

    private static void ConfigureBasicProperties(EntityTypeBuilder<Service> builder)
    {
        builder.Property(service => service.Name)
            .IsRequired();
        
        builder.Property(service => service.Description)
            .IsRequired();
        
        builder.Property(service => service.Price)
            .IsRequired();
    }

    private static void ConfigureDiscountRelation(EntityTypeBuilder<Service> builder)
    {
        builder.HasOne<Discount>(service => service.Discount)
            .WithMany("_services")
            .HasForeignKey(service => service.DiscountId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
        
        builder.Navigation(service => service.Discount)
            .UsePropertyAccessMode(PropertyAccessMode.Property);
    }

    private static void ConfigureIndexes(EntityTypeBuilder<Service> builder)
    {
        builder.HasIndex(service => service.Name)
            .IsUnique();
    }
}
