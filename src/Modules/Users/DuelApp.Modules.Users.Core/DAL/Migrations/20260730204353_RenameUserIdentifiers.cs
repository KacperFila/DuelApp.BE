using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuelApp.Modules.Users.Core.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserIdentifiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "KeycloakUserId",
                schema: "users",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "users",
                table: "Users",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Id",
                schema: "users",
                table: "Users",
                newName: "IX_Users_UserId");

            migrationBuilder.AddColumn<Guid>(
                name: "ProfileId",
                schema: "users",
                table: "Users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                schema: "users",
                table: "Users",
                column: "ProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfileId",
                schema: "users",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "users",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Users_UserId",
                schema: "users",
                table: "Users",
                newName: "IX_Users_Id");

            migrationBuilder.AddColumn<string>(
                name: "KeycloakUserId",
                schema: "users",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                schema: "users",
                table: "Users",
                column: "Id");
        }
    }
}
