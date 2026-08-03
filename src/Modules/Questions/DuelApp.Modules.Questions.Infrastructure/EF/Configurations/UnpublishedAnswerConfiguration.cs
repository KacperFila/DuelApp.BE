using DuelApp.Modules.Questions.Domain.Questions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DuelApp.Modules.Questions.Infrastructure.EF.Configurations;

internal sealed class UnpublishedAnswerConfiguration
    : IEntityTypeConfiguration<UnpublishedAnswer>
{
    public void Configure(EntityTypeBuilder<UnpublishedAnswer> builder)
    {
        builder.ToTable("unpublished_answers", table =>
        {
            table.HasCheckConstraint(
                "CK_unpublished_answers_source_position_non_negative",
                "\"SourcePosition\" >= 0");

            table.HasCheckConstraint(
                "CK_unpublished_answers_content_not_blank",
                "length(btrim(\"Content\")) > 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.UnpublishedQuestionId)
            .IsRequired();

        builder.Property(x => x.SourcePosition)
            .IsRequired();

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.IsCorrect)
            .IsRequired();

        builder.HasIndex(x => new { x.UnpublishedQuestionId, x.SourcePosition })
            .IsUnique();

        builder.HasOne(x => x.UnpublishedQuestion)
            .WithMany(x => x.Answers)
            .HasForeignKey(x => x.UnpublishedQuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
