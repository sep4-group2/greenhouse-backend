﻿// <auto-generated />
using System;
using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250524103838_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Data.Entities.Action", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("GreenhouseId")
                        .HasColumnType("int");

                    b.Property<bool>("Status")
                        .HasColumnType("bit");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime2");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("GreenhouseId");

                    b.ToTable("Actions");
                });

            modelBuilder.Entity("Data.Entities.Device", b =>
                {
                    b.Property<int>("DeviceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("DeviceId"));

                    b.Property<string>("Auth")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Endpoint")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("P256dh")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserEmail")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("DeviceId");

                    b.HasIndex("UserEmail");

                    b.ToTable("Devices");
                });

            modelBuilder.Entity("Data.Entities.Greenhouse", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("ActivePresetId")
                        .HasColumnType("int");

                    b.Property<string>("FertilizationMethod")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LightingMethod")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MacAddress")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserEmail")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("WateringMethod")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("ActivePresetId");

                    b.HasIndex("UserEmail");

                    b.ToTable("Greenhouses");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            ActivePresetId = 1,
                            FertilizationMethod = "manual",
                            LightingMethod = "manual",
                            MacAddress = "FF:9A:4C:98:6E:17",
                            Name = "Default Greenhouse",
                            UserEmail = "bob@smartgrow.nothing",
                            WateringMethod = "manual"
                        });
                });

            modelBuilder.Entity("Data.Entities.Preset", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("HoursOfLight")
                        .HasColumnType("int");

                    b.Property<double>("MaxAirHumidity")
                        .HasColumnType("float");

                    b.Property<double>("MaxSoilHumidity")
                        .HasColumnType("float");

                    b.Property<double>("MaxTemperature")
                        .HasColumnType("float");

                    b.Property<double>("MinAirHumidity")
                        .HasColumnType("float");

                    b.Property<double>("MinSoilHumidity")
                        .HasColumnType("float");

                    b.Property<double>("MinTemperature")
                        .HasColumnType("float");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("SystemPresetId")
                        .HasColumnType("int");

                    b.Property<int?>("UserPresetId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Presets");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            HoursOfLight = 12,
                            MaxAirHumidity = 60.0,
                            MaxSoilHumidity = 50.0,
                            MaxTemperature = 25.0,
                            MinAirHumidity = 40.0,
                            MinSoilHumidity = 30.0,
                            MinTemperature = 18.0,
                            Name = "Default System Preset",
                            SystemPresetId = 1
                        });
                });

            modelBuilder.Entity("Data.Entities.User", b =>
                {
                    b.Property<string>("email")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("email");

                    b.ToTable("Users");

                    b.HasData(
                        new
                        {
                            email = "bob@smartgrow.nothing",
                            Password = "AQAAAAIAAYagAAAAEDYARcRmYHoWH6vaS2iNLm5nA8hbhelY6ie7l9JZarybfFcBko+tUpqRBRg3m02loQ=="
                        });
                });

            modelBuilder.Entity("Notification", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("GreenhouseId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("GreenhouseId");

                    b.ToTable("Notifications");
                });

            modelBuilder.Entity("NotificationUser", b =>
                {
                    b.Property<int>("NotificationId")
                        .HasColumnType("int");

                    b.Property<string>("UserId")
                        .HasMaxLength(450)
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("NotificationId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("NotificationUser");
                });

            modelBuilder.Entity("SensorReading", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("GreenhouseId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime2");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Unit")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("Value")
                        .HasColumnType("float");

                    b.HasKey("Id");

                    b.HasIndex("GreenhouseId");

                    b.ToTable("SensorReadings");
                });

            modelBuilder.Entity("SystemPreset", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("SystemPresets");

                    b.HasData(
                        new
                        {
                            Id = 1
                        });
                });

            modelBuilder.Entity("UserPreset", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("UserEmail")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserEmail");

                    b.ToTable("UserPresets");
                });

            modelBuilder.Entity("Data.Entities.Action", b =>
                {
                    b.HasOne("Data.Entities.Greenhouse", "Greenhouse")
                        .WithMany("Actions")
                        .HasForeignKey("GreenhouseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Greenhouse");
                });

            modelBuilder.Entity("Data.Entities.Device", b =>
                {
                    b.HasOne("Data.Entities.User", "User")
                        .WithMany("Devices")
                        .HasForeignKey("UserEmail")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Data.Entities.Greenhouse", b =>
                {
                    b.HasOne("Data.Entities.Preset", "ActivePreset")
                        .WithMany("Greenhouses")
                        .HasForeignKey("ActivePresetId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Data.Entities.User", "User")
                        .WithMany("Greenhouses")
                        .HasForeignKey("UserEmail")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ActivePreset");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Notification", b =>
                {
                    b.HasOne("Data.Entities.Greenhouse", "Greenhouse")
                        .WithMany("Notifications")
                        .HasForeignKey("GreenhouseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Greenhouse");
                });

            modelBuilder.Entity("NotificationUser", b =>
                {
                    b.HasOne("Notification", null)
                        .WithMany()
                        .HasForeignKey("NotificationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Data.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("SensorReading", b =>
                {
                    b.HasOne("Data.Entities.Greenhouse", "Greenhouse")
                        .WithMany("SensorReadings")
                        .HasForeignKey("GreenhouseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Greenhouse");
                });

            modelBuilder.Entity("SystemPreset", b =>
                {
                    b.HasOne("Data.Entities.Preset", "Preset")
                        .WithOne("SystemPreset")
                        .HasForeignKey("SystemPreset", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Preset");
                });

            modelBuilder.Entity("UserPreset", b =>
                {
                    b.HasOne("Data.Entities.Preset", "Preset")
                        .WithOne("UserPreset")
                        .HasForeignKey("UserPreset", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Data.Entities.User", "User")
                        .WithMany("UserPresets")
                        .HasForeignKey("UserEmail")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Preset");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Data.Entities.Greenhouse", b =>
                {
                    b.Navigation("Actions");

                    b.Navigation("Notifications");

                    b.Navigation("SensorReadings");
                });

            modelBuilder.Entity("Data.Entities.Preset", b =>
                {
                    b.Navigation("Greenhouses");

                    b.Navigation("SystemPreset")
                        .IsRequired();

                    b.Navigation("UserPreset")
                        .IsRequired();
                });

            modelBuilder.Entity("Data.Entities.User", b =>
                {
                    b.Navigation("Devices");

                    b.Navigation("Greenhouses");

                    b.Navigation("UserPresets");
                });
#pragma warning restore 612, 618
        }
    }
}
