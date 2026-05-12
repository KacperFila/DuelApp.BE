using QuestionGenerator.Models;

namespace QuestionGenerator.Services;

public static class QuestionGenerator
{
    public static List<QuestionWithAnswers> GenerateMathQuestions(int count)
    {
        var random = new Random();
        var questions = new List<QuestionWithAnswers>();

        for (int i = 0; i < count; i++)
        {
            var isAddition = random.Next(0, 2) == 0;

            var a = random.Next(1, 50);
            var b = random.Next(1, 50);

            var correct = isAddition ? a + b : a - b;
            var questionText = isAddition
                ? $"What is {a} + {b}?"
                : $"What is {a} - {b}?";

            var answers = GenerateAnswers(correct, random);

            questions.Add(new QuestionWithAnswers
            {
                Title = questionText,
                Answers = answers
            });
        }

        return questions;
    }
    
    private static List<Answer> GenerateAnswers(int correct, Random random)
    {
        var answers = new List<int>
            {
                correct,
                correct + 1,
                correct - 1,
                correct + 10
            }
            .Where(x => x >= 0)
            .Distinct()
            .Take(4)
            .ToList();

        return answers
            .OrderBy(_ => random.Next())
            .Select(x => new Answer
            {
                Content = x.ToString(),
                IsCorrect = x == correct
            })
            .ToList();
    }
}