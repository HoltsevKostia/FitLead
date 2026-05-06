using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitLead.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorInvitationsToTokenBased : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invitations_users_ClientId",
                table: "invitations");

            migrationBuilder.DropIndex(
                name: "IX_invitations_ClientId",
                table: "invitations");

            migrationBuilder.DropIndex(
                name: "IX_invitations_TrainerId_ClientId_Status",
                table: "invitations");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "invitations");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "invitations",
                newName: "ExpiresAtUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "invitations",
                newName: "CreatedAtUtc");

            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAtUtc",
                table: "invitations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AcceptedByClientId",
                table: "invitations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenHash",
                table: "invitations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_AcceptedByClientId",
                table: "invitations",
                column: "AcceptedByClientId");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_Status_ExpiresAtUtc",
                table: "invitations",
                columns: new[] { "Status", "ExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_invitations_TokenHash",
                table: "invitations",
                column: "TokenHash",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_invitations_users_AcceptedByClientId",
                table: "invitations",
                column: "AcceptedByClientId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invitations_users_AcceptedByClientId",
                table: "invitations");

            migrationBuilder.DropIndex(
                name: "IX_invitations_AcceptedByClientId",
                table: "invitations");

            migrationBuilder.DropIndex(
                name: "IX_invitations_Status_ExpiresAtUtc",
                table: "invitations");

            migrationBuilder.DropIndex(
                name: "IX_invitations_TokenHash",
                table: "invitations");

            migrationBuilder.DropColumn(
                name: "AcceptedAtUtc",
                table: "invitations");

            migrationBuilder.DropColumn(
                name: "AcceptedByClientId",
                table: "invitations");

            migrationBuilder.DropColumn(
                name: "TokenHash",
                table: "invitations");

            migrationBuilder.RenameColumn(
                name: "ExpiresAtUtc",
                table: "invitations",
                newName: "ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "invitations",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "invitations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_invitations_ClientId",
                table: "invitations",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_TrainerId_ClientId_Status",
                table: "invitations",
                columns: new[] { "TrainerId", "ClientId", "Status" },
                unique: true,
                filter: "\"Status\" = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_invitations_users_ClientId",
                table: "invitations",
                column: "ClientId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
