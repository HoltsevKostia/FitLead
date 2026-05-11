using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitLead.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IntroduceTrainingProgramWeekDayStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_training_program_workouts_TrainingProgramId",
                table: "training_program_workouts");

            migrationBuilder.RenameColumn(
                name: "Order",
                table: "training_program_workouts",
                newName: "WeekNumber");

            migrationBuilder.AddColumn<int>(
                name: "DaysPerWeek",
                table: "training_programs",
                type: "integer",
                nullable: false,
                defaultValue: 7);

            migrationBuilder.AddColumn<int>(
                name: "WeeksCount",
                table: "training_programs",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "DayNumber",
                table: "training_program_workouts",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "OrderInDay",
                table: "training_program_workouts",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddCheckConstraint(
                name: "CK_training_programs_days_per_week_range",
                table: "training_programs",
                sql: "\"DaysPerWeek\" BETWEEN 1 AND 7");

            migrationBuilder.AddCheckConstraint(
                name: "CK_training_programs_weeks_count_range",
                table: "training_programs",
                sql: "\"WeeksCount\" BETWEEN 1 AND 24");

            migrationBuilder.Sql("""
                ALTER TABLE "training_program_workouts"
                ADD CONSTRAINT "UQ_training_program_workouts_program_day_order"
                UNIQUE ("TrainingProgramId", "WeekNumber", "DayNumber", "OrderInDay")
                DEFERRABLE INITIALLY DEFERRED;
                """);

            migrationBuilder.AddCheckConstraint(
                name: "CK_training_program_workouts_day_number_positive",
                table: "training_program_workouts",
                sql: "\"DayNumber\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_training_program_workouts_order_in_day_positive",
                table: "training_program_workouts",
                sql: "\"OrderInDay\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_training_program_workouts_week_number_positive",
                table: "training_program_workouts",
                sql: "\"WeekNumber\" > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_training_programs_days_per_week_range",
                table: "training_programs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_training_programs_weeks_count_range",
                table: "training_programs");

            migrationBuilder.Sql("""
                ALTER TABLE "training_program_workouts"
                DROP CONSTRAINT "UQ_training_program_workouts_program_day_order";
                """);

            migrationBuilder.DropCheckConstraint(
                name: "CK_training_program_workouts_day_number_positive",
                table: "training_program_workouts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_training_program_workouts_order_in_day_positive",
                table: "training_program_workouts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_training_program_workouts_week_number_positive",
                table: "training_program_workouts");

            migrationBuilder.DropColumn(
                name: "DaysPerWeek",
                table: "training_programs");

            migrationBuilder.DropColumn(
                name: "WeeksCount",
                table: "training_programs");

            migrationBuilder.DropColumn(
                name: "DayNumber",
                table: "training_program_workouts");

            migrationBuilder.DropColumn(
                name: "OrderInDay",
                table: "training_program_workouts");

            migrationBuilder.RenameColumn(
                name: "WeekNumber",
                table: "training_program_workouts",
                newName: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_training_program_workouts_TrainingProgramId",
                table: "training_program_workouts",
                column: "TrainingProgramId");
        }
    }
}
