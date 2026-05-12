using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuelApp.Modules.Matchmaking.Infrastructure.EF.Migrations
{
    /// <inheritdoc />
    public partial class AdjustIndexesForQueueEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_queue_entries_PlayerId",
                schema: "Matchmaking",
                table: "queue_entries");

            migrationBuilder.DropIndex(
                name: "IX_queue_entries_Status",
                schema: "Matchmaking",
                table: "queue_entries");

            migrationBuilder.DropIndex(
                name: "IX_queue_entries_Status_StartedAt",
                schema: "Matchmaking",
                table: "queue_entries");

            migrationBuilder.CreateIndex(
                name: "IX_queue_entries_PlayerId_Status",
                schema: "Matchmaking",
                table: "queue_entries",
                columns: new[] { "PlayerId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_queue_entries_PlayerId_Status",
                schema: "Matchmaking",
                table: "queue_entries");

            migrationBuilder.CreateIndex(
                name: "IX_queue_entries_PlayerId",
                schema: "Matchmaking",
                table: "queue_entries",
                column: "PlayerId",
                unique: true);

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
    }
}
