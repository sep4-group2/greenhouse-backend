using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Action = Data.Entities.Action;

namespace Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    // Add your DbSet properties for your entity classes here
    // public DbSet<YourEntity> YourEntities { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // User-Preset one-to-many (UserPreset)
        modelBuilder.Entity<UserPreset>()
            .HasOne(up => up.User)
            .WithMany(u => u.UserPresets)
            .HasForeignKey(up => up.UserEmail);

        // User-Greenhouse one-to-many
        modelBuilder.Entity<Greenhouse>()
            .HasOne(g => g.User)
            .WithMany(u => u.Greenhouses)
            .HasForeignKey(g => g.UserEmail);

        // Greenhouse-Preset one-to-many (ActivePreset)
        modelBuilder.Entity<Greenhouse>()
            .HasOne(g => g.ActivePreset)
            .WithMany(p => p.Greenhouses)
            .HasForeignKey(g => g.ActivePresetId)
            .OnDelete(DeleteBehavior.Restrict);

        // Greenhouse-SensorReading one-to-many
        modelBuilder.Entity<SensorReading>()
            .HasOne(sr => sr.Greenhouse)
            .WithMany(g => g.SensorReadings)
            .HasForeignKey(sr => sr.GreenhouseId);

        // Greenhouse-Notification one-to-many
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Greenhouse)
            .WithMany(g => g.Notifications)
            .HasForeignKey(n => n.GreenhouseId);

        // SystemPreset one-to-one with Preset
        modelBuilder.Entity<SystemPreset>()
            .HasOne(sp => sp.Preset)
            .WithOne()
            .HasForeignKey<SystemPreset>(sp => sp.Id);

        // UserPreset one-to-one with Preset
        modelBuilder.Entity<UserPreset>()
            .HasOne(up => up.Preset)
            .WithOne()
            .HasForeignKey<UserPreset>(up => up.Id);
        
        // Action
        modelBuilder.Entity<Action>()
            .HasOne(a => a.Greenhouse)
            .WithMany(g => g.Actions)
            .HasForeignKey(a => a.GreenhouseId);
        
        modelBuilder.Entity<Preset>().HasData(new Preset
        {
            Id = 1,
            Name = "Default System Preset",
            MinTemperature = 18.0,
            MaxTemperature = 25.0,
            MinAirHumidity = 40.0,
            MaxAirHumidity = 60.0,
            MinSoilHumidity = 30.0,
            MaxSoilHumidity = 50.0,
            HoursOfLight = 12
        });

        modelBuilder.Entity<SystemPreset>().HasData(new SystemPreset
        {
            Id = 1 // Matches the Preset ID
        });
       
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Greenhouse> Greenhouses { get; set; }
    public DbSet<Preset> Presets { get; set; }
    public DbSet<SystemPreset> SystemPresets { get; set; }
    public DbSet<UserPreset> UserPresets { get; set; }
    public DbSet<SensorReading> SensorReadings { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    
    public DbSet<Action> Actions { get; set; }


 
}