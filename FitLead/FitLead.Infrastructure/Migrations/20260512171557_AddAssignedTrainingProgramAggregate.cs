using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitLead.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedTrainingProgramAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assigned_training_programs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AccessSource = table.Column<int>(type: "integer", nullable: false),
                    AssignedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assigned_training_programs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_assigned_training_programs_training_programs_TrainingProgra~",
                        column: x => x.TrainingProgramId,
                        principalTable: "training_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_assigned_training_programs_users_ClientId",
                        column: x => x.ClientId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_assigned_training_programs_users_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_assigned_training_programs_client_id_status",
                table: "assigned_training_programs",
                columns: new[] { "ClientId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assigned_training_programs_trainer_id_client_id",
                table: "assigned_training_programs",
                columns: new[] { "TrainerId", "ClientId" });

            migrationBuilder.CreateIndex(
                name: "IX_assigned_training_programs_training_program_id",
                table: "assigned_training_programs",
                column: "TrainingProgramId");

            migrationBuilder.CreateIndex(
                name: "UX_assigned_training_programs_active_client_program",
                table: "assigned_training_programs",
                columns: new[] { "ClientId", "TrainingProgramId" },
                unique: true,
                filter: "\"Status\" = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assigned_training_programs");
        }
    }
}
