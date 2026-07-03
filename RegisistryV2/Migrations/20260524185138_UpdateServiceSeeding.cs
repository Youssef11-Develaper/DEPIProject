using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegisistryV2.Migrations
{
    /// <inheritdoc />
    public partial class UpdateServiceSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ServiceTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "RequiredDocuments",
                value: "بطاقة الرقم القومي منتهية الصلاحية\nصورة شخصية حديثة\nشهادة الميلاد");

            migrationBuilder.UpdateData(
                table: "ServiceTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "RequiredDocuments",
                value: "بطاقة الرقم القومي للأب أو الأم\nعقد الزواج\nشهادة الميلاد الأصلية من المستشفى");

            migrationBuilder.UpdateData(
                table: "ServiceTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "RequiredDocuments",
                value: "بطاقة الرقم القومي للطرفين\nشهادة ميلاد الطرفين\nموافقة ولي الأمر لو العروسة أقل من 21 سنة");

            migrationBuilder.UpdateData(
                table: "ServiceTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "RequiredDocuments",
                value: "بطاقة الرقم القومي للمتوفي\nتقرير الوفاة من المستشفى أو الطبيب\nبطاقة الرقم القومي لمقدم الطلب");

            migrationBuilder.UpdateData(
                table: "ServiceTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "RequiredDocuments",
                value: "بطاقة الرقم القومي\nعقد الزواج");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ServiceTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "RequiredDocuments",
                value: null);

            migrationBuilder.UpdateData(
                table: "ServiceTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "RequiredDocuments",
                value: null);

            migrationBuilder.UpdateData(
                table: "ServiceTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "RequiredDocuments",
                value: null);

            migrationBuilder.UpdateData(
                table: "ServiceTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "RequiredDocuments",
                value: null);

            migrationBuilder.UpdateData(
                table: "ServiceTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "RequiredDocuments",
                value: null);
        }
    }
}
