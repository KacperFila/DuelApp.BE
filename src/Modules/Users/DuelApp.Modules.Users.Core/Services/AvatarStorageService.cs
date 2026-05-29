using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using DuelApp.Modules.Users.Core.Constants;

namespace DuelApp.Modules.Users.Core.Services;

public class AvatarStorageService : IAvatarStorageService
{
    private readonly BlobContainerClient _container;

    public AvatarStorageService(BlobServiceClient blobServiceClient)
    {
        _container = blobServiceClient.GetBlobContainerClient(UserProfileConstants.ProfilePicturesContainerName);
    }

    public async Task UploadAsync(string blobName, Stream content, string contentType)
    {
        var blob = _container.GetBlobClient(blobName);

        await blob.UploadAsync(content, overwrite: true);

        await blob.SetHttpHeadersAsync(new BlobHttpHeaders
        {
            ContentType = contentType
        });
    }

    public string GetBlobUrl(string blobName)
    {
        var blobClient = _container.GetBlobClient(blobName);

        return blobClient.Uri.ToString();
    }
    
    public string GetAvatarUrl(Guid userId)
    {
        var userBlobName = $"users/{userId}.png";
        var defaultBlobName = "users/default.png";

        var blobClient = _container.GetBlobClient(userBlobName);

        var targetBlobName = blobClient.Exists() ? userBlobName : defaultBlobName;

        var targetBlob = _container.GetBlobClient(targetBlobName);

        if (!targetBlob.CanGenerateSasUri)
        {
            throw new InvalidOperationException("SAS generation not available (missing credentials).");
        }

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _container.Name,
            BlobName = targetBlobName,
            Resource = "b", // b -> Blob
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(5)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasUri = targetBlob.GenerateSasUri(sasBuilder);

        return sasUri.ToString();
    }
}