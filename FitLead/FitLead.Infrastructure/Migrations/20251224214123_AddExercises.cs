using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitLead.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExercises : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercise_Workout_WorkoutId",
                table: "Exercise");

            migrationBuilder.DropTable(
                name: "Workout");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Exercise",
                table: "Exercise");

            migrationBuilder.DropIndex(
                name: "IX_Exercise_WorkoutId",
                table: "Exercise");

            migrationBuilder.DropColumn(
                name: "Repetitions",
                table: "Exercise");

            migrationBuilder.DropColumn(
                name: "Sets",
                table: "Exercise");

            migrationBuilder.DropColumn(
                name: "WorkoutId",
                table: "Exercise");

            migrationBuilder.RenameTable(
                name: "Exercise",
                newName: "exercises");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "exercises",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "exercises",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MediaUrl",
                table: "exercises",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TrainerId",
                table: "exercises",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_exercises",
                table: "exercises",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "TrainingProgramWorkout",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    TrainingProgramId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingProgramWorkout", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingProgramWorkout_training_programs_TrainingProgramId",
                        column: x => x.TrainingProgramId,
                        principalTable: "training_programs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_exercises_TrainerId",
                table: "exercises",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingProgramWorkout_TrainingProgramId",
                table: "TrainingProgramWorkout",
                column: "TrainingProgramId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrainingProgramWorkout");

            migrationBuilder.DropPrimaryKey(
                name: "PK_exercises",
                table: "exercises");

            migrationBuilder.DropIndex(
                name: "IX_exercises_TrainerId",
                table: "exercises");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "exercises");

            migrationBuilder.DropColumn(
                name: "MediaUrl",
                table: "exercises");

            migrationBuilder.DropColumn(
                name: "TrainerId",
                table: "exercises");

            migrationBuilder.RenameTable(
                name: "exercises",
                newName: "Exercise");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Exercise",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<int>(
                name: "Repetitions",
                table: "Exercise",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Sets",
                table: "Exercise",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkoutId",
                table: "Exercise",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Exercise",
                table: "Exercise",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Workout",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TrainingProgramId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workout", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workout_training_programs_TrainingProgramId",
                        column: x => x.TrainingProgramId,
                        principalTable: "training_programs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_WorkoutId",
                table: "Exercise",
                column: "WorkoutId");

            migrationBuilder.CreateIndex(
                name: "IX_Workout_TrainingProgramId",
                table: "Workout",
                column: "TrainingProgramId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercise_Workout_WorkoutId",
                table: "Exercise",
                column: "WorkoutId",
                principalTable: "Workout",
                principalColumn: "Id");
        }
    }
}
