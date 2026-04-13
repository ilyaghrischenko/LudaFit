using LudaFit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LudaFit.Infrastructure.SQLite.Configurations;

public sealed class MedicineEntityConfiguration : IEntityTypeConfiguration<Medicine>
{
    public void Configure(EntityTypeBuilder<Medicine> builder)
    {
        builder.ToTable("Medicines");
        
        builder.HasKey(medicine => medicine.Id);
        
        builder.Property(medicine => medicine.Name)
            .IsRequired();

        builder.HasIndex(medicine => medicine.Name)
            .IsUnique();
    }
}
