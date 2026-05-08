using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitLead.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseClassificationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Equipment",
                table: "exercises",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MuscleGroup",
                table: "exercises",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_exercises_Equipment",
                table: "exercises",
                column: "Equipment");

            migrationBuilder.CreateIndex(
                name: "IX_exercises_MuscleGroup",
                table: "exercises",
                column: "MuscleGroup");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_exercises_Equipment",
                table: "exercises");

            migrationBuilder.DropIndex(
                name: "IX_exercises_MuscleGroup",
                table: "exercises");

            migrationBuilder.DropColumn(
                name: "Equipment",
                table: "exercises");

            migrationBuilder.DropColumn(
                name: "MuscleGroup",
                table: "exercises");
        }
    }
}
