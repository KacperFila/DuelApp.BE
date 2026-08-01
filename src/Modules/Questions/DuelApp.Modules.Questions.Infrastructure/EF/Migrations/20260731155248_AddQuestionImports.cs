using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuelApp.Modules.Questions.Infrastructure.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionImports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "question_imports",
                schema: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BlobName = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    BlobETag = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Uploaded"),
                    TotalQuestionsCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ProcessedQuestionsCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RejectedQuestionsCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ErrorMessage = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_question_imports", x => x.Id);
                    table.CheckConstraint("CK_question_imports_counts_non_negative", "\"TotalQuestionsCount\" >= 0\r\nAND \"ProcessedQuestionsCount\" >= 0\r\nAND \"RejectedQuestionsCount\" >= 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_question_imports_BlobName_BlobETag",
                schema: "Questions",
                table: "question_imports",
                columns: new[] { "BlobName", "BlobETag" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_question_imports_RequestedBy_CreatedAtUtc",
                schema: "Questions",
                table: "question_imports",
                columns: new[] { "RequestedBy", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_question_imports_Status_CreatedAtUtc",
                schema: "Questions",
                table: "question_imports",
                columns: new[] { "Status", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "question_imports",
                schema: "Questions");
        }
    }
}
