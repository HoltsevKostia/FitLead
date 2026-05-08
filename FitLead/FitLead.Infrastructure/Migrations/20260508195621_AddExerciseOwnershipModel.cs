using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitLead.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseOwnershipModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_exercises_users_TrainerId",
                table: "exercises");

            migrationBuilder.DropIndex(
                name: "IX_exercises_TrainerId",
                table: "exercises");

            migrationBuilder.RenameColumn(
                name: "TrainerId",
                table: "exercises",
                newName: "OwnerTrainerId");
            
            migrationBuilder.AddColumn<Guid>(
                name: "CopiedFromExerciseId",
                table: "exercises",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Source",
                table: "exercises",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.CreateIndex(
                name: "IX_exercises_CopiedFromExerciseId",
                table: "exercises",
                column: "CopiedFromExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_exercises_OwnerTrainerId",
                table: "exercises",
                column: "OwnerTrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_exercises_OwnerTrainerId_CopiedFromExerciseId",
                table: "exercises",
                columns: new[] { "OwnerTrainerId", "CopiedFromExerciseId" },
                unique: true,
                filter: "\"CopiedFromExerciseId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_exercises_Source",
                table: "exercises",
                column: "Source");

            migrationBuilder.AddCheckConstraint(
                name: "CK_exercises_source_valid",
                table: "exercises",
                sql: "\"Source\" IN (1, 2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_exercises_platform_owner_null",
                table: "exercises",
                sql: "\"Source\" <> 1 OR \"OwnerTrainerId\" IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_exercises_trainer_owner_required",
                table: "exercises",
                sql: "\"Source\" <> 2 OR \"OwnerTrainerId\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_exercises_copied_from_trainer_only",
                table: "exercises",
                sql: "\"CopiedFromExerciseId\" IS NULL OR \"Source\" = 2");

            migrationBuilder.AddForeignKey(
                name: "FK_exercises_exercises_CopiedFromExerciseId",
                table: "exercises",
                column: "CopiedFromExerciseId",
                principalTable: "exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_exercises_users_OwnerTrainerId",
                table: "exercises",
                column: "OwnerTrainerId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_exercises_exercises_CopiedFromExerciseId",
                table: "exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_exercises_users_OwnerTrainerId",
                table: "exercises");

            migrationBuilder.DropIndex(
                name: "IX_exercises_CopiedFromExerciseId",
                table: "exercises");

            migrationBuilder.DropIndex(
                name: "IX_exercises_OwnerTrainerId",
                table: "exercises");

            migrationBuilder.DropIndex(
                name: "IX_exercises_OwnerTrainerId_CopiedFromExerciseId",
                table: "exercises");

            migrationBuilder.DropIndex(
                name: "IX_exercises_Source",
                table: "exercises");

            migrationBuilder.DropCheckConstraint(
                name: "CK_exercises_copied_from_trainer_only",
                table: "exercises");

            migrationBuilder.DropCheckConstraint(
                name: "CK_exercises_platform_owner_null",
                table: "exercises");

            migrationBuilder.DropCheckConstraint(
                name: "CK_exercises_trainer_owner_required",
                table: "exercises");

            migrationBuilder.DropColumn(
                name: "CopiedFromExerciseId",
                table: "exercises");

            migrationBuilder.DropColumn(
                name: "OwnerTrainerId",
                table: "exercises");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "exercises");

            migrationBuilder.AddColumn<Guid>(
                name: "TrainerId",
                table: "exercises",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_exercises_TrainerId",
                table: "exercises",
                column: "TrainerId");

            migrationBuilder.AddForeignKey(
                name: "FK_exercises_users_TrainerId",
                table: "exercises",
                column: "TrainerId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
