using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitLead.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Fix_TrainingProgramWorkout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_training_program_workouts_training_programs_training_progra~",
                table: "training_program_workouts");

            migrationBuilder.RenameColumn(
                name: "training_program_id",
                table: "training_program_workouts",
                newName: "TrainingProgramId");

            migrationBuilder.RenameIndex(
                name: "IX_training_program_workouts_training_program_id",
                table: "training_program_workouts",
                newName: "IX_training_program_workouts_TrainingProgramId");

            migrationBuilder.AddForeignKey(
                name: "FK_training_program_workouts_training_programs_TrainingProgram~",
                table: "training_program_workouts",
                column: "TrainingProgramId",
                principalTable: "training_programs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_training_program_workouts_training_programs_TrainingProgram~",
                table: "training_program_workouts");

            migrationBuilder.RenameColumn(
                name: "TrainingProgramId",
                table: "training_program_workouts",
                newName: "training_program_id");

            migrationBuilder.RenameIndex(
                name: "IX_training_program_workouts_TrainingProgramId",
                table: "training_program_workouts",
                newName: "IX_training_program_workouts_training_program_id");

            migrationBuilder.AddForeignKey(
                name: "FK_training_program_workouts_training_programs_training_progra~",
                table: "training_program_workouts",
                column: "training_program_id",
                principalTable: "training_programs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
