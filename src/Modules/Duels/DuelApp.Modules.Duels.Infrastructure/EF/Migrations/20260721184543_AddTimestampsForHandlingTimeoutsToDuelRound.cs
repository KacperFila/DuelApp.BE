using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuelApp.Modules.Duels.Infrastructure.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddTimestampsForHandlingTimeoutsToDuelRound : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiresOnUtc",
                schema: "Duels",
                table: "DuelRounds");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndsAt",
                schema: "Duels",
                table: "DuelRounds",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishedAt",
                schema: "Duels",
                table: "DuelRounds",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                schema: "Duels",
                table: "DuelRounds",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                schema: "Duels",
                table: "DuelRounds",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndsAt",
                schema: "Duels",
                table: "DuelRounds");

            migrationBuilder.DropColumn(
                name: "FinishedAt",
                schema: "Duels",
                table: "DuelRounds");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "Duels",
                table: "DuelRounds");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                schema: "Duels",
                table: "DuelRounds");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresOnUtc",
                schema: "Duels",
                table: "DuelRounds",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
