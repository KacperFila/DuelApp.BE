using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DuelApp.Modules.Duels.Infrastructure.EF.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Duels");

            migrationBuilder.CreateTable(
                name: "Duels",
                schema: "Duels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerOneId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerTwoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CurrentRound = table.Column<int>(type: "integer", nullable: false),
                    TotalRounds = table.Column<int>(type: "integer", nullable: false),
                    PlayerOneScore = table.Column<int>(type: "integer", nullable: false),
                    PlayerTwoScore = table.Column<int>(type: "integer", nullable: false),
                    WinnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Duels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DuelRounds",
                schema: "Duels",
                columns: table => new
                {
                    Number = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DuelId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    HasPlayerOneAnsweredCorrectly = table.Column<bool>(type: "boolean", nullable: true),
                    HasPlayerTwoAnsweredCorrectly = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuelRounds", x => new { x.DuelId, x.Number });
                    table.ForeignKey(
                        name: "FK_DuelRounds_Duels_DuelId",
                        column: x => x.DuelId,
                        principalSchema: "Duels",
                        principalTable: "Duels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Duels_PlayerOneId_Status",
                schema: "Duels",
                table: "Duels",
                columns: new[] { "PlayerOneId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Duels_PlayerTwoId_Status",
                schema: "Duels",
                table: "Duels",
                columns: new[] { "PlayerTwoId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DuelRounds",
                schema: "Duels");

            migrationBuilder.DropTable(
                name: "Duels",
                schema: "Duels");
        }
    }
}
