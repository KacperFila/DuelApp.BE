using DuelApp.Shared.Abstractions.Exceptions;

namespace DuelApp.Modules.Questions.Application.Exceptions;

public class InvalidQuestionsJsonFormatException : DuelAppException
{
    public InvalidQuestionsJsonFormatException() : base("Invalid questions JSON format.")
    {
    }
}