using DuelApp.Modules.Questions.Domain.Questions.Enums;
using DuelApp.Shared.Abstractions.Exceptions;

namespace DuelApp.Modules.Questions.Application.Exceptions;

public sealed class QuestionImportNotCompletedException(
    Guid importId,
    ImportStatus status)
    : DuelAppException($"Question import {importId} cannot be published because its status is {status}.");
