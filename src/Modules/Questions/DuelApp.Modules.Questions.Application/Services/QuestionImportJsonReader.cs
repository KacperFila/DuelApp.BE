using System.Runtime.CompilerServices;
using System.Text.Json;
using DuelApp.Modules.Questions.Application.Exceptions;
using DuelApp.Modules.Questions.Application.Models;

namespace DuelApp.Modules.Questions.Application.Services;

public sealed class QuestionImportJsonReader
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Streams questions from a JSON array without loading the entire file into memory.
    /// </summary>
    /// <param name="content">The stream containing a JSON array of question objects.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous read operation.</param>
    /// <returns>An asynchronous sequence of deserialized questions.</returns>
    /// <exception cref="InvalidQuestionImportException">
    /// Thrown when the JSON array contains a <see langword="null" /> element.
    /// </exception>
    /// <example>
    /// <code>
    /// await foreach (var question in reader.ReadAsync(content, cancellationToken))
    /// {
    ///     Process(question);
    /// }
    /// </code>
    /// </example>
    public async IAsyncEnumerable<GeneratedQuestion> ReadAsync(
        Stream content,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var question in JsonSerializer.DeserializeAsyncEnumerable<GeneratedQuestion>(
                           content,
                           SerializerOptions,
                           cancellationToken))
        {
            if (question is null)
            {
                throw new InvalidQuestionImportException("The import JSON must contain question objects.");
            }

            yield return question;
        }
    }
}
