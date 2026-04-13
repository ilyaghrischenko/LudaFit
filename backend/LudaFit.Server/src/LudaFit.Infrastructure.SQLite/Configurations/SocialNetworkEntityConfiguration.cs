using LudaFit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LudaFit.Infrastructure.SQLite.Configurations;

public sealed class SocialNetworkEntityConfiguration : IEntityTypeConfiguration<SocialNetwork>
{
    public void Configure(EntityTypeBuilder<SocialNetwork> builder)
    {
        builder.ToTable("SocialNetworks");
        
        builder.HasKey(socialNetwork => socialNetwork.Id);
        
        ConfigureBasicProperties(builder);
        
        ConfigureSpecialistRelation(builder);
    }

    private static void ConfigureBasicProperties(EntityTypeBuilder<SocialNetwork> builder)
    {
        builder.Property(socialNetwork => socialNetwork.Name)
            .IsRequired();
        
        builder.Property(socialNetwork => socialNetwork.Url)
            .IsRequired();

        builder.Property(socialNetwork => socialNetwork.PhotoUrl)
            .IsRequired();
    }

    private static void ConfigureSpecialistRelation(EntityTypeBuilder<SocialNetwork> builder)
    {
        builder.HasOne<Specialist>(socialNetwork => socialNetwork.Specialist)
            .WithMany("_socialNetworks")
            .HasForeignKey(socialNetwork => socialNetwork.SpecialistId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}
