using System;
using Microsoft.EntityFrameworkCore.Migrations;

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
                name: "Presets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinTemperature = table.Column<double>(type: "float", nullable: false),
                    MaxTemperature = table.Column<double>(type: "float", nullable: false),
                    MinAirHumidity = table.Column<double>(type: "float", nullable: false),
                    MaxAirHumidity = table.Column<double>(type: "float", nullable: false),
                    MinSoilHumidity = table.Column<double>(type: "float", nullable: false),
                    MaxSoilHumidity = table.Column<double>(type: "float", nullable: false),
                    HoursOfLight = table.Column<int>(type: "int", nullable: false),
                    SystemPresetId = table.Column<int>(type: "int", nullable: true),
                    UserPresetId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Presets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.email);
                });

            migrationBuilder.CreateTable(
                name: "SystemPresets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
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
                name: "Devices",
                columns: table => new
                {
                    DeviceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Endpoint = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    P256dh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Auth = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserEmail = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.DeviceId);
                    table.ForeignKey(
                        name: "FK_Devices_Users_UserEmail",
                        column: x => x.UserEmail,
                        principalTable: "Users",
                        principalColumn: "email",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Greenhouses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MacAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LightingMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WateringMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FertilizationMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserEmail = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ActivePresetId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Greenhouses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Greenhouses_Presets_ActivePresetId",
                        column: x => x.ActivePresetId,
                        principalTable: "Presets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Greenhouses_Users_UserEmail",
                        column: x => x.UserEmail,
                        principalTable: "Users",
                        principalColumn: "email",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPresets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    UserEmail = table.Column<string>(type: "nvarchar(450)", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Actions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GreenhouseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Actions_Greenhouses_GreenhouseId",
                        column: x => x.GreenhouseId,
                        principalTable: "Greenhouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GreenhouseId = table.Column<int>(type: "int", nullable: false)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<double>(type: "float", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GreenhouseId = table.Column<int>(type: "int", nullable: false)
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
                    NotificationId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationUser", x => new { x.NotificationId, x.UserId });
                    table.ForeignKey(
                        name: "FK_NotificationUser_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationUser_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "email",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Presets",
                columns: new[] { "Id", "HoursOfLight", "MaxAirHumidity", "MaxSoilHumidity", "MaxTemperature", "MinAirHumidity", "MinSoilHumidity", "MinTemperature", "Name", "SystemPresetId", "UserPresetId" },
                values: new object[] { 1, 12, 60.0, 50.0, 25.0, 40.0, 30.0, 18.0, "Default System Preset", 1, null });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "email", "Password" },
                values: new object[] { "bob@smartgrow.nothing", "AQAAAAIAAYagAAAAEDYARcRmYHoWH6vaS2iNLm5nA8hbhelY6ie7l9JZarybfFcBko+tUpqRBRg3m02loQ==" });

            migrationBuilder.InsertData(
                table: "Greenhouses",
                columns: new[] { "Id", "ActivePresetId", "FertilizationMethod", "LightingMethod", "MacAddress", "Name", "UserEmail", "WateringMethod" },
                values: new object[] { 1, 1, "manual", "manual", "FF:9A:4C:98:6E:17", "Default Greenhouse", "bob@smartgrow.nothing", "manual" });

            migrationBuilder.InsertData(
                table: "SystemPresets",
                column: "Id",
                value: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Actions_GreenhouseId",
                table: "Actions",
                column: "GreenhouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_UserEmail",
                table: "Devices",
                column: "UserEmail");

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
                name: "IX_NotificationUser_UserId",
                table: "NotificationUser",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SensorReadings_GreenhouseId",
                table: "SensorReadings",
                column: "GreenhouseId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPresets_UserEmail",
                table: "UserPresets",
                column: "UserEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Actions");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "NotificationUser");

            migrationBuilder.DropTable(
                name: "SensorReadings");

            migrationBuilder.DropTable(
                name: "SystemPresets");

            migrationBuilder.DropTable(
                name: "UserPresets");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Greenhouses");

            migrationBuilder.DropTable(
                name: "Presets");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
