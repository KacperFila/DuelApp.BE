using DuelApp.Modules.Questions.Domain.Questions.Entities;
using DuelApp.Modules.Questions.Domain.Questions.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DuelApp.Modules.Questions.Infrastructure.EF.Configurations;

internal sealed class QuestionImportConfiguration
    : IEntityTypeConfiguration<QuestionImport>
{
    public void Configure(EntityTypeBuilder<QuestionImport> builder)
    {
        builder.ToTable("question_imports", table =>
        {
            table.HasCheckConstraint(
                "CK_question_imports_counts_non_negative",
                """
                "TotalQuestionsCount" >= 0
                AND "ProcessedQuestionsCount" >= 0
                AND "RejectedQuestionsCount" >= 0
                """);
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.BlobName)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(x => x.BlobETag)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.RequestedBy)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .HasDefaultValue(ImportStatus.Uploaded)
            .IsRequired();

        builder.Property(x => x.TotalQuestionsCount)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.ProcessedQuestionsCount)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.RejectedQuestionsCount)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(4000);

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(x => x.CompletedAtUtc)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(x => new { x.RequestedBy, x.CreatedAtUtc });

        builder.HasIndex(x => new { x.Status, x.CreatedAtUtc });

        builder.HasIndex(x => new { x.BlobName, x.BlobETag })
            .IsUnique();
    }
}