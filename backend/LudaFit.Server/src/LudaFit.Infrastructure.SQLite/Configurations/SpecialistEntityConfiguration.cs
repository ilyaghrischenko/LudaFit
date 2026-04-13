using LudaFit.Domain.Entities;
using LudaFit.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LudaFit.Infrastructure.SQLite.Configurations;

public sealed class SpecialistEntityConfiguration : IEntityTypeConfiguration<Specialist>
{
    public void Configure(EntityTypeBuilder<Specialist> builder)
    {
        builder.ToTable("Specialists");

        builder.HasKey(specialist => specialist.Id);
        
        ConfigureBasicProperties(builder);
        
        ConfigureValueObjects(builder);
        
        ConfigureSocialNetworkRelation(builder);

        ConfigureIndexes(builder);
    }

    private static void ConfigureBasicProperties(EntityTypeBuilder<Specialist> builder)
    {
        builder.Property(specialist => specialist.Name)
            .IsRequired();

        builder.Property(specialist => specialist.PhotoUrl)
            .IsRequired();
        
        builder.Property(specialist => specialist.Description)
            .IsRequired();
    }

    private static void ConfigureValueObjects(EntityTypeBuilder<Specialist> builder)
    {
        builder.ComplexProperty<TimeRange>(specialist => specialist.WorkTime, workTime =>
        {
            workTime.Property(wt => wt.Start)
                .IsRequired()
                .HasColumnName("WorkTimeStart");

            workTime.Property(wt => wt.End)
                .IsRequired()
                .HasColumnName("WorkTimeEnd");
        });
    }

    private static void ConfigureSocialNetworkRelation(EntityTypeBuilder<Specialist> builder)
    {
        builder.HasMany<SocialNetwork>("_socialNetworks")
            .WithOne(socialNetwork => socialNetwork.Specialist)
            .HasForeignKey(socialNetwork => socialNetwork.SpecialistId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        builder.Ignore(specialist => specialist.SocialNetworks);
        
        builder.Navigation("_socialNetworks")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureIndexes(EntityTypeBuilder<Specialist> builder)
    {
        builder.HasIndex(specialist => specialist.Name)
            .IsUnique();
    }
}
