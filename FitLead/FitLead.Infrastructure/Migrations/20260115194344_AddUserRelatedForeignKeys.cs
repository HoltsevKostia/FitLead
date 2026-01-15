using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitLead.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRelatedForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_workouts_TrainerId",
                table: "workouts",
                column: "TrainerId");

            migrationBuilder.AddForeignKey(
                name: "FK_exercises_users_TrainerId",
                table: "exercises",
                column: "TrainerId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_invitations_users_ClientId",
                table: "invitations",
                column: "ClientId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_invitations_users_TrainerId",
                table: "invitations",
                column: "TrainerId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_trainer_clients_users_ClientId",
                table: "trainer_clients",
                column: "ClientId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_trainer_clients_users_TrainerId",
                table: "trainer_clients",
                column: "TrainerId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_training_programs_users_TrainerId",
                table: "training_programs",
                column: "TrainerId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_workouts_users_TrainerId",
                table: "workouts",
                column: "TrainerId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_exercises_users_TrainerId",
                table: "exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_invitations_users_ClientId",
                table: "invitations");

            migrationBuilder.DropForeignKey(
                name: "FK_invitations_users_TrainerId",
                table: "invitations");

            migrationBuilder.DropForeignKey(
                name: "FK_trainer_clients_users_ClientId",
                table: "trainer_clients");

            migrationBuilder.DropForeignKey(
                name: "FK_trainer_clients_users_TrainerId",
                table: "trainer_clients");

            migrationBuilder.DropForeignKey(
                name: "FK_training_programs_users_TrainerId",
                table: "training_programs");

            migrationBuilder.DropForeignKey(
                name: "FK_workouts_users_TrainerId",
                table: "workouts");

            migrationBuilder.DropIndex(
                name: "IX_workouts_TrainerId",
                table: "workouts");
        }
    }
}
