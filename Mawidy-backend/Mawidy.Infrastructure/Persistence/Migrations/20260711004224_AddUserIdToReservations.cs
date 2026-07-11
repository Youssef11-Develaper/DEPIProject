using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mawidy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "HospitalReservations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "HospitalReservations");
        }
    }
}
