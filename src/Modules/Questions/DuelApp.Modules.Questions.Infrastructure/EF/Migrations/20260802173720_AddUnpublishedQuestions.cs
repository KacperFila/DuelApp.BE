using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuelApp.Modules.Questions.Infrastructure.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddUnpublishedQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "unpublished_questions",
                schema: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionImportId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourcePosition = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    AnswerIds = table.Column<List<Guid>>(type: "uuid[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_unpublished_questions", x => x.Id);
                    table.CheckConstraint("CK_unpublished_questions_source_position_non_negative", "\"SourcePosition\" >= 0");
                    table.CheckConstraint("CK_unpublished_questions_title_not_blank", "length(btrim(\"Title\")) > 0");
                    table.ForeignKey(
                        name: "FK_unpublished_questions_question_imports_QuestionImportId",
                        column: x => x.QuestionImportId,
                        principalSchema: "Questions",
                        principalTable: "question_imports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "unpublished_answers",
                schema: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UnpublishedQuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourcePosition = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_unpublished_answers", x => x.Id);
                    table.CheckConstraint("CK_unpublished_answers_content_not_blank", "length(btrim(\"Content\")) > 0");
                    table.CheckConstraint("CK_unpublished_answers_source_position_non_negative", "\"SourcePosition\" >= 0");
                    table.ForeignKey(
                        name: "FK_unpublished_answers_unpublished_questions_UnpublishedQuesti~",
                        column: x => x.UnpublishedQuestionId,
                        principalSchema: "Questions",
                        principalTable: "unpublished_questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_unpublished_answers_UnpublishedQuestionId_SourcePosition",
                schema: "Questions",
                table: "unpublished_answers",
                columns: new[] { "UnpublishedQuestionId", "SourcePosition" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_unpublished_questions_QuestionImportId_SourcePosition",
                schema: "Questions",
                table: "unpublished_questions",
                columns: new[] { "QuestionImportId", "SourcePosition" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "unpublished_answers",
                schema: "Questions");

            migrationBuilder.DropTable(
                name: "unpublished_questions",
                schema: "Questions");
        }
    }
}
