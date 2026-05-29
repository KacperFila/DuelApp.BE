using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuelApp.Modules.Users.Core.DAL.Migrations
{
    /// <inheritdoc />
    public partial class IntegrateUsersWithKeycloak : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Claims",
                schema: "users",
                table: "Users",
                newName: "KeycloakUserId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "users",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "KeycloakUserId",
                schema: "users",
                table: "Users",
                newName: "Claims");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "users",
                table: "Users",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }
    }
}
