using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitLead.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaAssetFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "media_assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageProvider = table.Column<int>(type: "integer", nullable: false),
                    StorageObjectId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DeliveryUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ContentType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_assets", x => x.Id);
                    table.CheckConstraint("CK_media_assets_duration_seconds_positive", "\"DurationSeconds\" IS NULL OR \"DurationSeconds\" > 0");
                    table.CheckConstraint("CK_media_assets_size_bytes_positive", "\"SizeBytes\" > 0");
                    table.ForeignKey(
                        name: "FK_media_assets_users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_media_assets_owner_user_id",
                table: "media_assets",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_media_assets_status",
                table: "media_assets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "UX_media_assets_storage_provider_object_id",
                table: "media_assets",
                columns: new[] { "StorageProvider", "StorageObjectId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "media_assets");
        }
    }
}
