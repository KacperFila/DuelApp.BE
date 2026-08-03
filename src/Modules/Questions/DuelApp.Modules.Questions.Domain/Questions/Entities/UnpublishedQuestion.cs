namespace DuelApp.Modules.Questions.Domain.Questions.Entities;

public class UnpublishedQuestion
{
    public Guid Id { get; set; }
    public Guid QuestionImportId { get; set; }
    public QuestionImport QuestionImport { get; set; } = null!;
    public int SourcePosition { get; set; }
    public string Title { get; set; } = null!;
    public List<Guid> AnswerIds { get; set; } = [];
    public List<UnpublishedAnswer> Answers { get; set; } = [];
}
