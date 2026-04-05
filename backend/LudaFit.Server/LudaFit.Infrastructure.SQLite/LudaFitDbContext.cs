using LudaFit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LudaFit.Infrastructure.SQLite;

public sealed class LudaFitDbContext : DbContext
{
    public LudaFitDbContext() { }
    
    public LudaFitDbContext(DbContextOptions<LudaFitDbContext> options)
        : base(options) { }
    
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Diagnosis> Diagnoses { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<Medicine> Medicines { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<SocialNetwork> SocialNetworks { get; set; }
    public DbSet<Specialist> Specialists { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LudaFitDbContext).Assembly);
    }
}
