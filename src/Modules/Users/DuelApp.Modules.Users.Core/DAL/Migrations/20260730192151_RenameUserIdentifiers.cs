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
            migrationBuilder.DropIndex(
                name: "IX_Users_Id",
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
                newName: "ProfileId");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                schema: "users",
                table: "Users",
                type: "text",
                nullable: true);

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
            migrationBuilder.DropIndex(
                name: "IX_Users_UserId",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "users",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "ProfileId",
                schema: "users",
                table: "Users",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "KeycloakUserId",
                schema: "users",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Id",
                schema: "users",
                table: "Users",
                column: "Id",
                unique: true);
        }
    }
}
