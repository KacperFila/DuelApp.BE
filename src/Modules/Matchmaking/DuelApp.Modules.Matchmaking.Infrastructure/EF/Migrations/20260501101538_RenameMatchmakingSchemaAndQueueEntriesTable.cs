using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuelApp.Modules.Matchmaking.Infrastructure.EF.Migrations
{
    /// <inheritdoc />
    public partial class RenameMatchmakingSchemaAndQueueEntriesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "matchmaking_queue_entries",
                schema: "MatchmakingQueueEntries");

            migrationBuilder.EnsureSchema(
                name: "Matchmaking");

            migrationBuilder.CreateTable(
                name: "queue_entries",
                schema: "Matchmaking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_queue_entries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_queue_entries_PlayerId",
                schema: "Matchmaking",
                table: "queue_entries",
                column: "PlayerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_queue_entries_StartedAt",
                schema: "Matchmaking",
                table: "queue_entries",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_queue_entries_Status",
                schema: "Matchmaking",
                table: "queue_entries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_queue_entries_Status_StartedAt",
                schema: "Matchmaking",
                table: "queue_entries",
                columns: new[] { "Status", "StartedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "queue_entries",
                schema: "Matchmaking");

            migrationBuilder.EnsureSchema(
                name: "MatchmakingQueueEntries");

            migrationBuilder.CreateTable(
                name: "matchmaking_queue_entries",
                schema: "MatchmakingQueueEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matchmaking_queue_entries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_matchmaking_queue_entries_PlayerId",
                schema: "MatchmakingQueueEntries",
                table: "matchmaking_queue_entries",
                column: "PlayerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_matchmaking_queue_entries_StartedAt",
                schema: "MatchmakingQueueEntries",
                table: "matchmaking_queue_entries",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_matchmaking_queue_entries_Status",
                schema: "MatchmakingQueueEntries",
                table: "matchmaking_queue_entries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_matchmaking_queue_entries_Status_StartedAt",
                schema: "MatchmakingQueueEntries",
                table: "matchmaking_queue_entries",
                columns: new[] { "Status", "StartedAt" });
        }
    }
}
