using LudaFit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LudaFit.Infrastructure.SQLite.Configurations;

public sealed class AdminEntityConfiguration : IEntityTypeConfiguration<Admin>
{
    public void Configure(EntityTypeBuilder<Admin> builder)
    {
        builder.ToTable("Admins");
        
        builder.HasKey(admin => admin.Id);

        builder.Property(admin => admin.Login)
            .IsRequired();
        
        builder.Property(admin => admin.PasswordHash)
            .IsRequired();
    }
}
