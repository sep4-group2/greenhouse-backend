using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class FixPresets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Presets_SystemPresets_SystemPresetId",
                table: "Presets");

            migrationBuilder.DropForeignKey(
                name: "FK_Presets_UserPresets_UserPresetId",
                table: "Presets");

            migrationBuilder.AlterColumn<int>(
                name: "UserPresetId",
                table: "Presets",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "SystemPresetId",
                table: "Presets",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.InsertData(
                table: "Presets",
                columns: new[] { "Id", "HoursOfLight", "MaxAirHumidity", "MaxSoilHumidity", "MaxTemperature", "MinAirHumidity", "MinSoilHumidity", "MinTemperature", "Name", "SystemPresetId", "UserPresetId" },
                values: new object[] { 1, 12, 60.0, 50.0, 25.0, 40.0, 30.0, 18.0, "Default System Preset", null, null });

            migrationBuilder.InsertData(
                table: "SystemPresets",
                column: "Id",
                value: 1);

            migrationBuilder.AddForeignKey(
                name: "FK_Presets_SystemPresets_SystemPresetId",
                table: "Presets",
                column: "SystemPresetId",
                principalTable: "SystemPresets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Presets_UserPresets_UserPresetId",
                table: "Presets",
                column: "UserPresetId",
                principalTable: "UserPresets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Presets_SystemPresets_SystemPresetId",
                table: "Presets");

            migrationBuilder.DropForeignKey(
                name: "FK_Presets_UserPresets_UserPresetId",
                table: "Presets");

            migrationBuilder.DeleteData(
                table: "SystemPresets",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Presets",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "UserPresetId",
                table: "Presets",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SystemPresetId",
                table: "Presets",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

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
    }
}
