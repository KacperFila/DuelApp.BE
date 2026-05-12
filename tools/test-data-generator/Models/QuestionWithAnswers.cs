namespace QuestionGenerator.Models;

public class QuestionWithAnswers
{
    public string Title { get; set; }
    public List<Answer> Answers { get; set; }
}