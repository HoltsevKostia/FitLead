using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitLead.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClientProfilesModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "client_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Goal = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExperienceLevel = table.Column<int>(type: "integer", nullable: true),
                    HeightCm = table.Column<int>(type: "integer", nullable: true),
                    Limitations = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TrainingPreferences = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AdditionalInfo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_profiles", x => x.Id);
                    table.CheckConstraint("CK_client_profiles_experience_level_valid", "\"ExperienceLevel\" IS NULL OR \"ExperienceLevel\" IN (1, 2, 3)");
                    table.CheckConstraint("CK_client_profiles_height_range", "\"HeightCm\" IS NULL OR (\"HeightCm\" BETWEEN 50 AND 300)");
                    table.CheckConstraint("CK_client_profiles_updated_at_after_created", "\"UpdatedAtUtc\" IS NULL OR \"UpdatedAtUtc\" >= \"CreatedAtUtc\"");
                    table.ForeignKey(
                        name: "FK_client_profiles_users_ClientId",
                        column: x => x.ClientId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UX_client_profiles_client_id",
                table: "client_profiles",
                column: "ClientId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "client_profiles");
        }
    }
}
