using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitLead.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoReportToChatMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VideoReportId",
                table: "chat_messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_chat_messages_video_report_id",
                table: "chat_messages",
                column: "VideoReportId",
                unique: true,
                filter: "\"VideoReportId\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_chat_messages_video_report_id_null_for_text_type",
                table: "chat_messages",
                sql: "\"Type\" <> 1 OR \"VideoReportId\" IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_chat_messages_video_report_shape",
                table: "chat_messages",
                sql: "\"Type\" <> 3 OR (\"VideoReportId\" IS NOT NULL AND \"Text\" IS NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_chat_messages_video_reports_VideoReportId",
                table: "chat_messages",
                column: "VideoReportId",
                principalTable: "video_reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_chat_messages_video_reports_VideoReportId",
                table: "chat_messages");

            migrationBuilder.DropIndex(
                name: "UX_chat_messages_video_report_id",
                table: "chat_messages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_chat_messages_video_report_id_null_for_text_type",
                table: "chat_messages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_chat_messages_video_report_shape",
                table: "chat_messages");

            migrationBuilder.DropColumn(
                name: "VideoReportId",
                table: "chat_messages");
        }
    }
}
