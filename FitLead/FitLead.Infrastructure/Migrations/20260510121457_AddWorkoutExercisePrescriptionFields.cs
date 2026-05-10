using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitLead.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutExercisePrescriptionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LoadKg",
                table: "workout_exercises",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "workout_exercises",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "TrainerNote",
                table: "workout_exercises",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_workout_exercises_workout_id_Order",
                table: "workout_exercises",
                columns: new[] { "workout_id", "Order" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_workout_exercises_order_positive",
                table: "workout_exercises",
                sql: "\"Order\" > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_workout_exercises_workout_id_Order",
                table: "workout_exercises");

            migrationBuilder.DropColumn(
                name: "LoadKg",
                table: "workout_exercises");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "workout_exercises");

            migrationBuilder.DropColumn(
                name: "TrainerNote",
                table: "workout_exercises");
        }
    }
}
