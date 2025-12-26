using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitLead.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingProgramWorkouts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrainingProgramWorkout_training_programs_TrainingProgramId",
                table: "TrainingProgramWorkout");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrainingProgramWorkout",
                table: "TrainingProgramWorkout");

            migrationBuilder.RenameTable(
                name: "TrainingProgramWorkout",
                newName: "training_program_workouts");

            migrationBuilder.RenameColumn(
                name: "TrainingProgramId",
                table: "training_program_workouts",
                newName: "training_program_id");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingProgramWorkout_TrainingProgramId",
                table: "training_program_workouts",
                newName: "IX_training_program_workouts_training_program_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_training_program_workouts",
                table: "training_program_workouts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_training_program_workouts_training_programs_training_progra~",
                table: "training_program_workouts",
                column: "training_program_id",
                principalTable: "training_programs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_training_program_workouts_training_programs_training_progra~",
                table: "training_program_workouts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_training_program_workouts",
                table: "training_program_workouts");

            migrationBuilder.RenameTable(
                name: "training_program_workouts",
                newName: "TrainingProgramWorkout");

            migrationBuilder.RenameColumn(
                name: "training_program_id",
                table: "TrainingProgramWorkout",
                newName: "TrainingProgramId");

            migrationBuilder.RenameIndex(
                name: "IX_training_program_workouts_training_program_id",
                table: "TrainingProgramWorkout",
                newName: "IX_TrainingProgramWorkout_TrainingProgramId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrainingProgramWorkout",
                table: "TrainingProgramWorkout",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingProgramWorkout_training_programs_TrainingProgramId",
                table: "TrainingProgramWorkout",
                column: "TrainingProgramId",
                principalTable: "training_programs",
                principalColumn: "Id");
        }
    }
}
