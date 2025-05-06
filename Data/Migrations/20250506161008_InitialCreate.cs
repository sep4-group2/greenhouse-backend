using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    email = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.email);
                });

            migrationBuilder.CreateTable(
                name: "Greenhouses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: false),
                    LightingMethod = table.Column<string>(type: "text", nullable: false),
                    WateringMethod = table.Column<string>(type: "text", nullable: false),
                    FertilizationMethod = table.Column<string>(type: "text", nullable: false),
                    UserEmail = table.Column<string>(type: "text", nullable: false),
                    ActivePresetId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Greenhouses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Greenhouses_Users_UserEmail",
                        column: x => x.UserEmail,
                        principalTable: "Users",
                        principalColumn: "email",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    GreenhouseId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Greenhouses_GreenhouseId",
                        column: x => x.GreenhouseId,
                        principalTable: "Greenhouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SensorReadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GreenhouseId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SensorReadings_Greenhouses_GreenhouseId",
                        column: x => x.GreenhouseId,
                        principalTable: "Greenhouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationUser",
                columns: table => new
                {
                    NotificationsId = table.Column<int>(type: "integer", nullable: false),
                    Usersemail = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationUser", x => new { x.NotificationsId, x.Usersemail });
                    table.ForeignKey(
                        name: "FK_NotificationUser_Notifications_NotificationsId",
                        column: x => x.NotificationsId,
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationUser_Users_Usersemail",
                        column: x => x.Usersemail,
                        principalTable: "Users",
                        principalColumn: "email",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Presets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    MinTemperature = table.Column<double>(type: "double precision", nullable: false),
                    MaxTemperature = table.Column<double>(type: "double precision", nullable: false),
                    MinAirHumidity = table.Column<double>(type: "double precision", nullable: false),
                    MaxAirHumidity = table.Column<double>(type: "double precision", nullable: false),
                    MinSoilHumidity = table.Column<double>(type: "double precision", nullable: false),
                    MaxSoilHumidity = table.Column<double>(type: "double precision", nullable: false),
                    HoursOfLight = table.Column<int>(type: "integer", nullable: false),
                    SystemPresetId = table.Column<int>(type: "integer", nullable: false),
                    UserPresetId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Presets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemPresets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemPresets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemPresets_Presets_Id",
                        column: x => x.Id,
                        principalTable: "Presets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPresets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    UserEmail = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPresets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPresets_Presets_Id",
                        column: x => x.Id,
                        principalTable: "Presets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPresets_Users_UserEmail",
                        column: x => x.UserEmail,
                        principalTable: "Users",
                        principalColumn: "email",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Greenhouses_ActivePresetId",
                table: "Greenhouses",
                column: "ActivePresetId");

            migrationBuilder.CreateIndex(
                name: "IX_Greenhouses_UserEmail",
                table: "Greenhouses",
                column: "UserEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_GreenhouseId",
                table: "Notifications",
                column: "GreenhouseId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationUser_Usersemail",
                table: "NotificationUser",
                column: "Usersemail");

            migrationBuilder.CreateIndex(
                name: "IX_Presets_SystemPresetId",
                table: "Presets",
                column: "SystemPresetId");

            migrationBuilder.CreateIndex(
                name: "IX_Presets_UserPresetId",
                table: "Presets",
                column: "UserPresetId");

            migrationBuilder.CreateIndex(
                name: "IX_SensorReadings_GreenhouseId",
                table: "SensorReadings",
                column: "GreenhouseId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPresets_UserEmail",
                table: "UserPresets",
                column: "UserEmail");

            migrationBuilder.AddForeignKey(
                name: "FK_Greenhouses_Presets_ActivePresetId",
                table: "Greenhouses",
                column: "ActivePresetId",
                principalTable: "Presets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Presets_SystemPresets_SystemPresetId",
                table: "Presets",
                column: "SystemPresetId",
                principalTable: "SystemPresets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Presets_UserPresets_UserPresetId",
                table: "Presets",
                column: "UserPresetId",
                principalTable: "UserPresets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SystemPresets_Presets_Id",
                table: "SystemPresets");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPresets_Presets_Id",
                table: "UserPresets");

            migrationBuilder.DropTable(
                name: "NotificationUser");

            migrationBuilder.DropTable(
                name: "SensorReadings");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Greenhouses");

            migrationBuilder.DropTable(
                name: "Presets");

            migrationBuilder.DropTable(
                name: "SystemPresets");

            migrationBuilder.DropTable(
                name: "UserPresets");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
