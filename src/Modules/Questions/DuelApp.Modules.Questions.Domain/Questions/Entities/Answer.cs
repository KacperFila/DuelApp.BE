namespace DuelApp.Modules.Questions.Domain.Questions.Entities;

public class Answer
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public string Content { get; set; }
    public bool IsCorrect { get; set; }
}