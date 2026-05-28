using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuelApp.Modules.Duels.Infrastructure.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertiesToCheckIfPlayerSubmittedAnswerForRound : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasPlayerOneSubmittedAnswer",
                schema: "Duels",
                table: "DuelRounds",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasPlayerTwoSubmittedAnswer",
                schema: "Duels",
                table: "DuelRounds",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasPlayerOneSubmittedAnswer",
                schema: "Duels",
                table: "DuelRounds");

            migrationBuilder.DropColumn(
                name: "HasPlayerTwoSubmittedAnswer",
                schema: "Duels",
                table: "DuelRounds");
        }
    }
}
