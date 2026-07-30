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

            migrationBuilder.DropIndex(
                name: "IX_Users_Id",
                schema: "users",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "users",
                table: "Users",
                newName: "ProfileId");

            migrationBuilder.RenameColumn(
                name: "KeycloakUserId",
                schema: "users",
                table: "Users",
                newName: "UserId");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                schema: "users",
                table: "Users",
                type: "uuid using \"UserId\"::uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                schema: "users",
                table: "Users",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserId",
                schema: "users",
                table: "Users",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                schema: "users",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserId",
                schema: "users",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "users",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "users",
                table: "Users",
                newName: "KeycloakUserId");

            migrationBuilder.RenameColumn(
                name: "ProfileId",
                schema: "users",
                table: "Users",
                newName: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                schema: "users",
                table: "Users",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Id",
                schema: "users",
                table: "Users",
                column: "Id",
                unique: true);
        }
    }
}
