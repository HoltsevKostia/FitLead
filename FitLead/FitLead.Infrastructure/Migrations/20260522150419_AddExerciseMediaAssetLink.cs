using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitLead.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseMediaAssetLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MediaUrl",
                table: "exercises");

            migrationBuilder.AddColumn<Guid>(
                name: "MediaAssetId",
                table: "exercises",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_exercises_MediaAssetId",
                table: "exercises",
                column: "MediaAssetId");

            migrationBuilder.AddForeignKey(
                name: "FK_exercises_media_assets_MediaAssetId",
                table: "exercises",
                column: "MediaAssetId",
                principalTable: "media_assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_exercises_media_assets_MediaAssetId",
                table: "exercises");

            migrationBuilder.DropIndex(
                name: "IX_exercises_MediaAssetId",
                table: "exercises");

            migrationBuilder.DropColumn(
                name: "MediaAssetId",
                table: "exercises");

            migrationBuilder.AddColumn<string>(
                name: "MediaUrl",
                table: "exercises",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);
        }
    }
}
