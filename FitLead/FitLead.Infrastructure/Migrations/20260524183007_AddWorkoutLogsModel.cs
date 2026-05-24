using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitLead.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutLogsModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "workout_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedTrainingProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingProgramWorkoutId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PerformedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClientNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DifficultyRating = table.Column<int>(type: "integer", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workout_logs", x => x.Id);
                    table.CheckConstraint("CK_workout_logs_completed_performed_at_required", "\"Status\" <> 1 OR \"PerformedAtUtc\" IS NOT NULL");
                    table.CheckConstraint("CK_workout_logs_difficulty_rating_range", "\"DifficultyRating\" IS NULL OR (\"DifficultyRating\" BETWEEN 1 AND 10)");
                    table.CheckConstraint("CK_workout_logs_skipped_fields_null", "\"Status\" <> 2 OR (\"PerformedAtUtc\" IS NULL AND \"DifficultyRating\" IS NULL)");
                    table.CheckConstraint("CK_workout_logs_status_valid", "\"Status\" IN (1, 2)");
                    table.CheckConstraint("CK_workout_logs_updated_at_after_created", "\"UpdatedAtUtc\" IS NULL OR \"UpdatedAtUtc\" >= \"CreatedAtUtc\"");
                    table.ForeignKey(
                        name: "FK_workout_logs_assigned_training_programs_AssignedTrainingPro~",
                        column: x => x.AssignedTrainingProgramId,
                        principalTable: "assigned_training_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workout_logs_training_program_workouts_TrainingProgramWorko~",
                        column: x => x.TrainingProgramWorkoutId,
                        principalTable: "training_program_workouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workout_logs_users_ClientId",
                        column: x => x.ClientId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_workout_logs_users_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_workout_logs_client_id",
                table: "workout_logs",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_workout_logs_status",
                table: "workout_logs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_workout_logs_trainer_id",
                table: "workout_logs",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_workout_logs_TrainingProgramWorkoutId",
                table: "workout_logs",
                column: "TrainingProgramWorkoutId");

            migrationBuilder.CreateIndex(
                name: "UX_workout_logs_assignment_program_workout",
                table: "workout_logs",
                columns: new[] { "AssignedTrainingProgramId", "TrainingProgramWorkoutId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workout_logs");
        }
    }
}
