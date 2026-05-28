using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuelApp.Modules.Questions.Infrastructure.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionPropertyToAnswer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_answers_questions_QuestionId",
                schema: "Questions",
                table: "answers",
                column: "QuestionId",
                principalSchema: "Questions",
                principalTable: "questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_answers_questions_QuestionId",
                schema: "Questions",
                table: "answers");
        }
    }
}
