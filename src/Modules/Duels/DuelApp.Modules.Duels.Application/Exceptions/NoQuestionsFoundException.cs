using DuelApp.Shared.Abstractions.Exceptions;

namespace DuelApp.Modules.Duels.Application.Exceptions;

internal class NoQuestionsFoundException : DuelAppException
{
    public NoQuestionsFoundException() : base("No questions found.")
    {
    }
}