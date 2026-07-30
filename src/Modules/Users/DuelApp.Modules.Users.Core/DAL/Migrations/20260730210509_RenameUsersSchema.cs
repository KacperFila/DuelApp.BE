using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuelApp.Modules.Users.Core.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RenameUsersSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "users",
                newName: "Users",
                newSchema: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "users");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "Users",
                newName: "Users",
                newSchema: "users");
        }
    }
}
