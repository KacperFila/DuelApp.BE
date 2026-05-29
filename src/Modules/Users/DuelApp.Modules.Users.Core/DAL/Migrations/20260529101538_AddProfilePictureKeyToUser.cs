using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuelApp.Modules.Users.Core.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddProfilePictureKeyToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfileImageKey",
                schema: "users",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileImageKey",
                schema: "users",
                table: "Users");
        }
    }
}
