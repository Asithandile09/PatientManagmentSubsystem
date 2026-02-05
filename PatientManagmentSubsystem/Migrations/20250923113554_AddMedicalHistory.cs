using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientManagmentSubsystem.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MedicalHistories",
                columns: table => new
                {
                    HistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    HasAllergies = table.Column<bool>(type: "bit", nullable: false),
                    AllergyDetails = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HasChronicIllness = table.Column<bool>(type: "bit", nullable: false),
                    ChronicIllnessDetails = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsPregnant = table.Column<bool>(type: "bit", nullable: false),
                    UsesContraceptives = table.Column<bool>(type: "bit", nullable: false),
                    Smokes = table.Column<bool>(type: "bit", nullable: false),
                    DrinksAlcohol = table.Column<bool>(type: "bit", nullable: false),
                    FamilyHeartDisease = table.Column<bool>(type: "bit", nullable: false),
                    FamilyDiabetes = table.Column<bool>(type: "bit", nullable: false),
                    FamilyCancer = table.Column<bool>(type: "bit", nullable: false),
                    OtherNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalHistories", x => x.HistoryId);
                    table.ForeignKey(
                        name: "FK_MedicalHistories_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistories_PatientId",
                table: "MedicalHistories",
                column: "PatientId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicalHistories");
        }
    }
}
