using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegisistryV2.Migrations
{
    /// <inheritdoc />
    public partial class AddGovernorateCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CenterLatitude",
                table: "Governorates",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CenterLongitude",
                table: "Governorates",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CenterLatitude", "CenterLongitude" },
                values: new object[] { 30.0626, 31.249700000000001 });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CenterLatitude", "CenterLongitude" },
                values: new object[] { 30.013100000000001, 31.2089 });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CenterLatitude", "CenterLongitude" },
                values: new object[] { 31.200099999999999, 29.918700000000001 });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CenterLatitude", "CenterLongitude" },
                values: new object[] { 30.7226, 31.723099999999999 });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CenterLatitude", "CenterLongitude" },
                values: new object[] { 31.040900000000001, 31.381900000000002 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CenterLatitude",
                table: "Governorates");

            migrationBuilder.DropColumn(
                name: "CenterLongitude",
                table: "Governorates");
        }
    }
}
