using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PatientManagmentSubsystem.Migrations
{
    /// <inheritdoc />
    public partial class AddWardEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Wards",
                columns: new[] { "WardId", "AvailableBeds", "Name", "TotalBeds" },
                values: new object[,]
                {
                    { 1, 20, "General Ward", 20 },
                    { 2, 20, "Emergency", 25 },
                    { 3, 10, "ICU", 10 },
                    { 4, 15, "Pediatric", 15 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Wards",
                keyColumn: "WardId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Wards",
                keyColumn: "WardId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Wards",
                keyColumn: "WardId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Wards",
                keyColumn: "WardId",
                keyValue: 4);
        }
    }
}
