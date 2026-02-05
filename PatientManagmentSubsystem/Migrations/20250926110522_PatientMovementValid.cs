using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientManagmentSubsystem.Migrations
{
    /// <inheritdoc />
    public partial class PatientMovementValid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ToBedId",
                table: "PatientMovements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ToWardId",
                table: "PatientMovements",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ToBedId",
                table: "PatientMovements");

            migrationBuilder.DropColumn(
                name: "ToWardId",
                table: "PatientMovements");
        }
    }
}
