namespace DuelApp.Modules.Questions.Domain.Questions.Entities;

public class Question
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public List<Guid> AnswerIds { get; set; }
    public List<Answer> Answers { get; set; }
}