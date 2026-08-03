namespace DuelApp.Modules.Questions.Domain.Questions.Entities;

public class UnpublishedAnswer
{
    public Guid Id { get; set; }
    public Guid UnpublishedQuestionId { get; set; }
    public UnpublishedQuestion UnpublishedQuestion { get; set; } = null!;
    public int SourcePosition { get; set; }
    public string Content { get; set; } = null!;
    public bool IsCorrect { get; set; }
}
