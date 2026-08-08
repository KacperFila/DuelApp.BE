using DuelApp.Shared.Abstractions.Exceptions;

namespace DuelApp.Modules.Questions.Application.Exceptions;

public sealed class QuestionImportNotFoundException(Guid importId)
    : DuelAppException($"Question import {importId} does not exist.");
