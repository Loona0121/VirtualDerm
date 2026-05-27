using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DermaAI.Migrations
{
    /// <inheritdoc />
    public partial class AddDiagnosisToConsultations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Diagnosis",
                table: "Consultations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Diagnosis",
                table: "Consultations");
        }
    }
}
