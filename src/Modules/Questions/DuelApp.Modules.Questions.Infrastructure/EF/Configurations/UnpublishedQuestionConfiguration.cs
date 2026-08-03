using DuelApp.Modules.Questions.Domain.Questions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DuelApp.Modules.Questions.Infrastructure.EF.Configurations;

internal sealed class UnpublishedQuestionConfiguration
    : IEntityTypeConfiguration<UnpublishedQuestion>
{
    public void Configure(EntityTypeBuilder<UnpublishedQuestion> builder)
    {
        builder.ToTable("unpublished_questions", table =>
        {
            table.HasCheckConstraint(
                "CK_unpublished_questions_source_position_non_negative",
                "\"SourcePosition\" >= 0");

            table.HasCheckConstraint(
                "CK_unpublished_questions_title_not_blank",
                "length(btrim(\"Title\")) > 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.QuestionImportId)
            .IsRequired();

        builder.Property(x => x.SourcePosition)
            .IsRequired();

        builder.Property(x => x.Title)
            .IsRequired();

        builder.Property(x => x.AnswerIds)
            .IsRequired();

        builder.HasIndex(x => new { x.QuestionImportId, x.SourcePosition })
            .IsUnique();

        builder.HasOne(x => x.QuestionImport)
            .WithMany(x => x.UnpublishedQuestions)
            .HasForeignKey(x => x.QuestionImportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
