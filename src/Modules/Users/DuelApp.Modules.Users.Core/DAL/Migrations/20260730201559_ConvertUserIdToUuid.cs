using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuelApp.Modules.Users.Core.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ConvertUserIdToUuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE users.\"Users\" ALTER COLUMN \"UserId\" TYPE uuid USING \"UserId\"::uuid;");
            migrationBuilder.Sql("ALTER TABLE users.\"Users\" ALTER COLUMN \"UserId\" SET NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE users.\"Users\" ALTER COLUMN \"UserId\" TYPE text USING \"UserId\"::text;");
            migrationBuilder.Sql("ALTER TABLE users.\"Users\" ALTER COLUMN \"UserId\" DROP NOT NULL;");
        }
    }
}
