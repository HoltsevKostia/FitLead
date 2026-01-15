using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitLead.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutAndExerciseForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "workout_id",
                table: "workout_exercises",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_workout_exercises_ExerciseId",
                table: "workout_exercises",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_training_program_workouts_WorkoutId",
                table: "training_program_workouts",
                column: "WorkoutId");

            migrationBuilder.AddForeignKey(
                name: "FK_training_program_workouts_workouts_WorkoutId",
                table: "training_program_workouts",
                column: "WorkoutId",
                principalTable: "workouts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_workout_exercises_exercises_ExerciseId",
                table: "workout_exercises",
                column: "ExerciseId",
                principalTable: "exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_training_program_workouts_workouts_WorkoutId",
                table: "training_program_workouts");

            migrationBuilder.DropForeignKey(
                name: "FK_workout_exercises_exercises_ExerciseId",
                table: "workout_exercises");

            migrationBuilder.DropIndex(
                name: "IX_workout_exercises_ExerciseId",
                table: "workout_exercises");

            migrationBuilder.DropIndex(
                name: "IX_training_program_workouts_WorkoutId",
                table: "training_program_workouts");

            migrationBuilder.AlterColumn<Guid>(
                name: "workout_id",
                table: "workout_exercises",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }
    }
}
