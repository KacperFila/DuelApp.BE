using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DuelApp.Modules.Questions.Application.Abstractions;
using DuelApp.Modules.Questions.Application.Models;
using DuelApp.Modules.Questions.Infrastructure.Const;
using Microsoft.Extensions.DependencyInjection;

namespace DuelApp.Modules.Questions.Infrastructure.Services;

public sealed class QuestionImportFileStorage
    : IQuestionImportFileStorage
{
    private readonly BlobContainerClient _containerClient;
    
    public QuestionImportFileStorage(
        [FromKeyedServices(BlobContainerClients.QuestionImports)]
        BlobContainerClient containerClient)
    {
        _containerClient = containerClient;
    }

    /// <summary>
    /// Uploads an import file and returns its storage metadata.
    /// </summary>
    /// <param name="content">The content stream to upload.</param>
    /// <param name="blobName">The unique path of the blob within the question-imports container.</param>
    /// <param name="cancellationToken">A token used to cancel the upload operation.</param>
    /// <returns>The stored blob name and its ETag.</returns>
    public async Task<StoredImportFile> UploadAsync(
        Stream content,
        string blobName,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(blobName);

        var blobClient = _containerClient.GetBlobClient(blobName);

        var response = await blobClient.UploadAsync(
            content,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = "application/json"
                }
            },
            cancellationToken);

        return new StoredImportFile(
            blobName,
            response.Value.ETag.ToString());
    }

    /// <summary>
    /// Opens an import file for streaming read.
    /// </summary>
    /// <param name="blobName">The path of the blob within the question-imports container.</param>
    /// <param name="cancellationToken">A token used to cancel the download operation.</param>
    /// <returns>A readable stream containing the blob content. The caller is responsible for disposing it.</returns>
    /// <example>
    /// <code>
    /// await using var content = await fileStorage.OpenReadAsync(blobName, cancellationToken);
    /// // Consume content while it is in scope.
    /// </code>
    /// </example>
    public async Task<Stream> OpenReadAsync(
        string blobName,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(blobName);

        var blobClient = _containerClient.GetBlobClient(blobName);

        var response = await blobClient.DownloadStreamingAsync(
            cancellationToken: cancellationToken);

        return response.Value.Content;
    }
}
