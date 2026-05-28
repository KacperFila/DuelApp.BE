using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuelApp.Modules.Duels.Infrastructure.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusAndNormalizeDuelRoundFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "HasPlayerTwoSubmittedAnswer",
                schema: "Duels",
                table: "DuelRounds",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "HasPlayerTwoAnsweredCorrectly",
                schema: "Duels",
                table: "DuelRounds",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "HasPlayerOneSubmittedAnswer",
                schema: "Duels",
                table: "DuelRounds",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "HasPlayerOneAnsweredCorrectly",
                schema: "Duels",
                table: "DuelRounds",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "Duels",
                table: "DuelRounds",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                schema: "Duels",
                table: "DuelRounds");

            migrationBuilder.AlterColumn<bool>(
                name: "HasPlayerTwoSubmittedAnswer",
                schema: "Duels",
                table: "DuelRounds",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "HasPlayerTwoAnsweredCorrectly",
                schema: "Duels",
                table: "DuelRounds",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "HasPlayerOneSubmittedAnswer",
                schema: "Duels",
                table: "DuelRounds",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "HasPlayerOneAnsweredCorrectly",
                schema: "Duels",
                table: "DuelRounds",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");
        }
    }
}
