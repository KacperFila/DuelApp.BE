using DuelApp.Shared.Abstractions.Exceptions;

namespace DuelApp.Modules.Duels.Application.Exceptions;

internal class QuestionNotFoundException : DuelAppException
{
    public Guid QuestionId { get; set;  }
    
    public QuestionNotFoundException(Guid questionId) : base($"No question found with id: {questionId}.")
    {
        QuestionId = questionId;
    }
}