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

        // User-Greenhouse one-to-many - KEEP CASCADE (default)
        modelBuilder.Entity<Greenhouse>()
            .HasOne(g => g.User)
            .WithMany(u => u.Greenhouses)
            .HasForeignKey(g => g.UserEmail);
        
        //User - Device one-to-many
        modelBuilder.Entity<Device>()
            .HasOne(d => d.User)
            .WithMany(u => u.Devices)
            .HasForeignKey(d => d.UserEmail);

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

        // Greenhouse-Notification one-to-many - KEEP CASCADE (default)
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Greenhouse)
            .WithMany(g => g.Notifications)
            .HasForeignKey(n => n.GreenhouseId);

        // Configure the many-to-many relationship between Notification and User
        // DISABLE CASCADE from User to NotificationUser
        modelBuilder.Entity<Notification>()
            .HasMany(n => n.Users)
            .WithMany(u => u.Notifications)
            .UsingEntity<Dictionary<string, object>>(
                "NotificationUser",
                j => j.HasOne<User>()
                    .WithMany()
                    .HasForeignKey("UserId")
                    .HasPrincipalKey(u => u.email)
                    .OnDelete(DeleteBehavior.Restrict), // Prevent cascade from User
                j => j.HasOne<Notification>()
                    .WithMany()
                    .HasForeignKey("NotificationId")
                    .OnDelete(DeleteBehavior.Cascade) // Allow cascade from Notification
            );

        // Fix the key length issue for the join table
        modelBuilder.Entity("NotificationUser", entity =>
        {
            entity.Property("UserId").HasMaxLength(450); // Limit the email length
        });

// SystemPreset one-to-one with Preset
        modelBuilder.Entity<SystemPreset>()
            .HasOne(sp => sp.Preset)
            .WithOne(p => p.SystemPreset) // Add navigation property to Preset
            .HasForeignKey<SystemPreset>(sp => sp.Id)
            .OnDelete(DeleteBehavior.Cascade);

// UserPreset one-to-one with Preset
        modelBuilder.Entity<UserPreset>()
            .HasOne(up => up.Preset)
            .WithOne(p => p.UserPreset) // Add navigation property to Preset
            .HasForeignKey<UserPreset>(up => up.Id)
            .OnDelete(DeleteBehavior.Cascade);

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

        //create a user
        modelBuilder.Entity<User>().HasData(new User
        {
            email = "bob@smartgrow.nothing",
            Password = "AQAAAAIAAYagAAAAEDYARcRmYHoWH6vaS2iNLm5nA8hbhelY6ie7l9JZarybfFcBko+tUpqRBRg3m02loQ=="
        });

        //create a greenhouse
        modelBuilder.Entity<Greenhouse>().HasData(new Greenhouse
        {
            Id = 1,
            Name = "Default Greenhouse",
            WateringMethod = "manual",
            LightingMethod = "manual",
            FertilizationMethod = "manual",
            MacAddress = "fc:f5:c4:87:00:24",
            UserEmail = "bob@smartgrow.nothing",
            ActivePresetId = 1,
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
    public DbSet<Device> Devices { get; set; }


 
}