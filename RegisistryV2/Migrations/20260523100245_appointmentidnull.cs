using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegisistryV2.Migrations
{
    /// <inheritdoc />
    public partial class appointmentidnull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_Appointments_AppointmentId",
                table: "Complaints");

            migrationBuilder.AlterColumn<int>(
                name: "AppointmentId",
                table: "Complaints",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Appointments_AppointmentId",
                table: "Complaints",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_Appointments_AppointmentId",
                table: "Complaints");

            migrationBuilder.AlterColumn<int>(
                name: "AppointmentId",
                table: "Complaints",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Appointments_AppointmentId",
                table: "Complaints",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
