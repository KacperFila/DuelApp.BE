using DuelApp.Modules.Questions.Application.Models;

namespace DuelApp.Modules.Questions.Application.Abstractions;

public interface IQuestionImportFileStorage
{
    Task<StoredImportFile> UploadAsync(
        Stream content,
        string blobName,
        CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(
        string blobName,
        CancellationToken cancellationToken);
}
