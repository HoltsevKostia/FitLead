using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitLead.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoReportModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "video_reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TrainerFeedbackText = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_video_reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_video_reports_chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_video_reports_users_ClientId",
                        column: x => x.ClientId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_video_reports_users_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "video_report_media",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderInReport = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_video_report_media", x => x.Id);
                    table.CheckConstraint("CK_video_report_media_order_in_report_positive", "\"OrderInReport\" > 0");
                    table.ForeignKey(
                        name: "FK_video_report_media_media_assets_MediaAssetId",
                        column: x => x.MediaAssetId,
                        principalTable: "media_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_video_report_media_video_reports_VideoReportId",
                        column: x => x.VideoReportId,
                        principalTable: "video_reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_video_report_media_MediaAssetId",
                table: "video_report_media",
                column: "MediaAssetId");

            migrationBuilder.CreateIndex(
                name: "UX_video_report_media_report_id_order_in_report",
                table: "video_report_media",
                columns: new[] { "VideoReportId", "OrderInReport" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_video_reports_chat_id",
                table: "video_reports",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_video_reports_client_id",
                table: "video_reports",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_video_reports_status",
                table: "video_reports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_video_reports_trainer_id",
                table: "video_reports",
                column: "TrainerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "video_report_media");

            migrationBuilder.DropTable(
                name: "video_reports");
        }
    }
}
