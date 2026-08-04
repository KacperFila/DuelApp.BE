using DuelApp.Shared.Abstractions.Exceptions;

namespace DuelApp.Modules.Questions.Application.Exceptions;

public sealed class InvalidQuestionImportException(string message)
    : DuelAppException(message);
