using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientManagmentSubsystem.Migrations
{
    /// <inheritdoc />
    public partial class DbSetPatientMovement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientMovement_Patients_PatientId",
                table: "PatientMovement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PatientMovement",
                table: "PatientMovement");

            migrationBuilder.RenameTable(
                name: "PatientMovement",
                newName: "PatientMovements");

            migrationBuilder.RenameIndex(
                name: "IX_PatientMovement_PatientId",
                table: "PatientMovements",
                newName: "IX_PatientMovements_PatientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PatientMovements",
                table: "PatientMovements",
                column: "PatientMovementId");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientMovements_Patients_PatientId",
                table: "PatientMovements",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientMovements_Patients_PatientId",
                table: "PatientMovements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PatientMovements",
                table: "PatientMovements");

            migrationBuilder.RenameTable(
                name: "PatientMovements",
                newName: "PatientMovement");

            migrationBuilder.RenameIndex(
                name: "IX_PatientMovements_PatientId",
                table: "PatientMovement",
                newName: "IX_PatientMovement_PatientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PatientMovement",
                table: "PatientMovement",
                column: "PatientMovementId");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientMovement_Patients_PatientId",
                table: "PatientMovement",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
