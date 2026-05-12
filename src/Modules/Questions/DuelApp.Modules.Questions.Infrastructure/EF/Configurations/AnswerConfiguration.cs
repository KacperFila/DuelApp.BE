using DuelApp.Modules.Questions.Domain.Questions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DuelApp.Modules.Questions.Infrastructure.EF.Configurations;

internal sealed class AnswerConfiguration : IEntityTypeConfiguration<Answer>
{
    public void Configure(EntityTypeBuilder<Answer> builder)
    {
        builder.ToTable("answers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Content)
            .IsRequired();
        
        builder.Property(x => x.IsCorrect)
            .IsRequired();
        
        builder.Property(x => x.QuestionId)
            .IsRequired();

        builder.HasIndex(x => x.QuestionId);
        
        builder.Property<byte[]>("RowVersion")
            .IsRowVersion();
    }
}