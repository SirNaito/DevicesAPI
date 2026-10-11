using Devices.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Devices.Api.Data;

public class DevicesDbContext : DbContext
{
    // Constructor for the DevicesDbContext class, which takes DbContextOptions as a parameter and passes it to the base DbContext class.
    public DevicesDbContext(
        DbContextOptions<DevicesDbContext> options)
        : base(options)
    {
    }
    // DbSet property for the Device entity, allowing CRUD operations on the Devices table in the database.
    public DbSet<Device> Devices => Set<Device>();

    // Configures the model for the Device entity, specifying primary key, required properties, maximum lengths, and indexes.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(device => device.Id);

            entity.Property(device => device.Name)
                .IsRequired()
                .HasMaxLength(120);

            entity.Property(device => device.Brand)
                .IsRequired()
                .HasMaxLength(80);

            entity.Property(device => device.State)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(device => device.CreatedAt)
                .IsRequired();

            entity.HasIndex(device => device.Brand);
            entity.HasIndex(device => device.State);
        });
    }
}