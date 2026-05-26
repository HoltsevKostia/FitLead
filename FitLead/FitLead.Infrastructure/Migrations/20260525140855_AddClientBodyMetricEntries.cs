using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitLead.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClientBodyMetricEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "client_body_metric_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedAt = table.Column<DateOnly>(type: "date", nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    BodyFatPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    ChestCm = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    WaistCm = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    HipsCm = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    ArmCm = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    ThighCm = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_body_metric_entries", x => x.Id);
                    table.CheckConstraint("CK_client_body_metric_entries_body_fat_range", "\"BodyFatPercent\" IS NULL OR (\"BodyFatPercent\" BETWEEN 1 AND 80)");
                    table.CheckConstraint("CK_client_body_metric_entries_measurements_range", "(\"ChestCm\" IS NULL OR (\"ChestCm\" BETWEEN 1 AND 300)) AND (\"WaistCm\" IS NULL OR (\"WaistCm\" BETWEEN 1 AND 300)) AND (\"HipsCm\" IS NULL OR (\"HipsCm\" BETWEEN 1 AND 300)) AND (\"ArmCm\" IS NULL OR (\"ArmCm\" BETWEEN 1 AND 300)) AND (\"ThighCm\" IS NULL OR (\"ThighCm\" BETWEEN 1 AND 300))");
                    table.CheckConstraint("CK_client_body_metric_entries_not_empty", "\"WeightKg\" IS NOT NULL OR \"BodyFatPercent\" IS NOT NULL OR \"ChestCm\" IS NOT NULL OR \"WaistCm\" IS NOT NULL OR \"HipsCm\" IS NOT NULL OR \"ArmCm\" IS NOT NULL OR \"ThighCm\" IS NOT NULL OR \"Note\" IS NOT NULL");
                    table.CheckConstraint("CK_client_body_metric_entries_updated_at_after_created", "\"UpdatedAtUtc\" IS NULL OR \"UpdatedAtUtc\" >= \"CreatedAtUtc\"");
                    table.CheckConstraint("CK_client_body_metric_entries_weight_range", "\"WeightKg\" IS NULL OR (\"WeightKg\" BETWEEN 1 AND 500)");
                    table.ForeignKey(
                        name: "FK_client_body_metric_entries_users_ClientId",
                        column: x => x.ClientId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UX_client_body_metric_entries_client_id_recorded_at",
                table: "client_body_metric_entries",
                columns: new[] { "ClientId", "RecordedAt" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "client_body_metric_entries");
        }
    }
}
