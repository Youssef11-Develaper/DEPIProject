using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RegisistryV2.Migrations
{
    /// <inheritdoc />
    public partial class seeddatainit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "AppointmentId",
                table: "Complaints",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "Governorates",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "القاهرة" },
                    { 2, "الجيزة" },
                    { 3, "الإسكندرية" },
                    { 4, "الشرقية" },
                    { 5, "الدقهلية" }
                });

            migrationBuilder.InsertData(
                table: "ServiceTypes",
                columns: new[] { "Id", "Description", "DurationMinutes", "Name" },
                values: new object[,]
                {
                    { 1, "استخراج أو تجديد بطاقة الرقم القومي", 30, "بطاقة الرقم القومي" },
                    { 2, "استخراج شهادة ميلاد", 20, "شهادة الميلاد" },
                    { 3, "توثيق عقد الزواج", 45, "عقد الزواج" },
                    { 4, "استخراج شهادة وفاة", 20, "شهادة الوفاة" },
                    { 5, "استخراج قيد الأسرة", 15, "قيد الأسرة" }
                });

            migrationBuilder.InsertData(
                table: "Branches",
                columns: new[] { "Id", "Address", "GovernorateId", "Latitude", "Longitude", "Name" },
                values: new object[,]
                {
                    { 1, "شارع الأهرام، مصر الجديدة", 1, 30.088999999999999, 31.3233, "سجل مدني مصر الجديدة" },
                    { 2, "شارع عباس العقاد، مدينة نصر", 1, 30.0626, 31.341100000000001, "سجل مدني مدينة نصر" },
                    { 3, "شارع التحرير، الدقي", 2, 30.040800000000001, 31.211099999999998, "سجل مدني الدقي" },
                    { 4, "شارع النصر، الإسكندرية", 3, 31.200099999999999, 29.918700000000001, "سجل مدني الإسكندرية" }
                });

            migrationBuilder.InsertData(
                table: "BranchSchedules",
                columns: new[] { "Id", "BranchId", "CloseTime", "DayOfWeek", "MaxAppointmentsPerSlot", "OpenTime", "PeakEndTime", "PeakStartTime" },
                values: new object[,]
                {
                    { 1, 1, new TimeSpan(0, 15, 0, 0, 0), 0, 3, new TimeSpan(0, 8, 0, 0, 0), new TimeSpan(0, 12, 0, 0, 0), new TimeSpan(0, 10, 0, 0, 0) },
                    { 2, 1, new TimeSpan(0, 15, 0, 0, 0), 1, 3, new TimeSpan(0, 8, 0, 0, 0), new TimeSpan(0, 12, 0, 0, 0), new TimeSpan(0, 10, 0, 0, 0) },
                    { 3, 1, new TimeSpan(0, 15, 0, 0, 0), 2, 3, new TimeSpan(0, 8, 0, 0, 0), new TimeSpan(0, 12, 0, 0, 0), new TimeSpan(0, 10, 0, 0, 0) },
                    { 4, 1, new TimeSpan(0, 15, 0, 0, 0), 3, 3, new TimeSpan(0, 8, 0, 0, 0), new TimeSpan(0, 12, 0, 0, 0), new TimeSpan(0, 10, 0, 0, 0) },
                    { 5, 1, new TimeSpan(0, 15, 0, 0, 0), 4, 3, new TimeSpan(0, 8, 0, 0, 0), new TimeSpan(0, 12, 0, 0, 0), new TimeSpan(0, 10, 0, 0, 0) },
                    { 6, 2, new TimeSpan(0, 15, 0, 0, 0), 0, 2, new TimeSpan(0, 8, 0, 0, 0), new TimeSpan(0, 11, 0, 0, 0), new TimeSpan(0, 9, 0, 0, 0) },
                    { 7, 2, new TimeSpan(0, 15, 0, 0, 0), 1, 2, new TimeSpan(0, 8, 0, 0, 0), new TimeSpan(0, 11, 0, 0, 0), new TimeSpan(0, 9, 0, 0, 0) },
                    { 8, 2, new TimeSpan(0, 15, 0, 0, 0), 2, 2, new TimeSpan(0, 8, 0, 0, 0), new TimeSpan(0, 11, 0, 0, 0), new TimeSpan(0, 9, 0, 0, 0) },
                    { 9, 2, new TimeSpan(0, 15, 0, 0, 0), 3, 2, new TimeSpan(0, 8, 0, 0, 0), new TimeSpan(0, 11, 0, 0, 0), new TimeSpan(0, 9, 0, 0, 0) },
                    { 10, 2, new TimeSpan(0, 15, 0, 0, 0), 4, 2, new TimeSpan(0, 8, 0, 0, 0), new TimeSpan(0, 11, 0, 0, 0), new TimeSpan(0, 9, 0, 0, 0) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_AppointmentId",
                table: "Complaints",
                column: "AppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Appointments_AppointmentId",
                table: "Complaints",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_Appointments_AppointmentId",
                table: "Complaints");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_AppointmentId",
                table: "Complaints");

            migrationBuilder.DeleteData(
                table: "BranchSchedules",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "BranchSchedules",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "BranchSchedules",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "BranchSchedules",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "BranchSchedules",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "BranchSchedules",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "BranchSchedules",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "BranchSchedules",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "BranchSchedules",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "BranchSchedules",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ServiceTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ServiceTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ServiceTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ServiceTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ServiceTypes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "AppointmentId",
                table: "Complaints",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
