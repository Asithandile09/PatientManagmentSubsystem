using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientManagmentSubsystem.Migrations
{
    /// <inheritdoc />
    public partial class movementType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FromWardId",
                table: "PatientMovements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "PatientMovements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "PatientMovements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PatientMovements_FromWardId",
                table: "PatientMovements",
                column: "FromWardId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientMovements_ToBedId",
                table: "PatientMovements",
                column: "ToBedId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientMovements_ToWardId",
                table: "PatientMovements",
                column: "ToWardId");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientMovements_Beds_ToBedId",
                table: "PatientMovements",
                column: "ToBedId",
                principalTable: "Beds",
                principalColumn: "BedId");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientMovements_Wards_FromWardId",
                table: "PatientMovements",
                column: "FromWardId",
                principalTable: "Wards",
                principalColumn: "WardId");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientMovements_Wards_ToWardId",
                table: "PatientMovements",
                column: "ToWardId",
                principalTable: "Wards",
                principalColumn: "WardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientMovements_Beds_ToBedId",
                table: "PatientMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_PatientMovements_Wards_FromWardId",
                table: "PatientMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_PatientMovements_Wards_ToWardId",
                table: "PatientMovements");

            migrationBuilder.DropIndex(
                name: "IX_PatientMovements_FromWardId",
                table: "PatientMovements");

            migrationBuilder.DropIndex(
                name: "IX_PatientMovements_ToBedId",
                table: "PatientMovements");

            migrationBuilder.DropIndex(
                name: "IX_PatientMovements_ToWardId",
                table: "PatientMovements");

            migrationBuilder.DropColumn(
                name: "FromWardId",
                table: "PatientMovements");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "PatientMovements");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "PatientMovements");
        }
    }
}
