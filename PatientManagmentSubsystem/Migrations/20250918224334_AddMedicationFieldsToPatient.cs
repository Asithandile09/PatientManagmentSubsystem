using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientManagmentSubsystem.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicationFieldsToPatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTakingMedication",
                table: "Patients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MedicationName",
                table: "Patients",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTakingMedication",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "MedicationName",
                table: "Patients");
        }
    }
}
