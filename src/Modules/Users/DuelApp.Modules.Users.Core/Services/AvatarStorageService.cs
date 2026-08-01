using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using DuelApp.Modules.Users.Core.Constants;
using DuelApp.Shared.Abstractions.Time;
using Microsoft.Extensions.DependencyInjection;

namespace DuelApp.Modules.Users.Core.Services;

public class AvatarStorageService : IAvatarStorageService
{
    private static readonly TimeSpan UserDelegationKeyLifetime = TimeSpan.FromHours(1);
    private static readonly TimeSpan UserDelegationKeyRefreshWindow = TimeSpan.FromMinutes(5);
    private const string DefaultBlobName = "users/default.png";
    
    private readonly BlobServiceClient _serviceClient;
    private readonly BlobContainerClient _container;
    private readonly IClock _clock;
    private readonly SemaphoreSlim _userDelegationKeyLock = new(1, 1);

    private UserDelegationKey? _userDelegationKey;
    private DateTimeOffset _userDelegationKeyExpiresOn;

    public AvatarStorageService(
        [FromKeyedServices(BlobServiceClients.ProfilePictures)]
        BlobServiceClient serviceClient,
        [FromKeyedServices(BlobContainerClients.ProfilePictures)]
        BlobContainerClient container,
        IClock clock)
    {
        _serviceClient = serviceClient;
        _container = container;
        _clock = clock;
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
    
    public async Task<string> GetAvatarUrlAsync(
        Guid profileId,
        CancellationToken cancellationToken = default)
    {
        var avatarBlob = await ResolveAvatarBlobAsync(profileId, cancellationToken);
        var sasUri = await CreateReadSasUriAsync(avatarBlob, cancellationToken);

        return sasUri.ToString();
    }

    private async Task<BlobClient> ResolveAvatarBlobAsync(
        Guid profileId,
        CancellationToken cancellationToken)
    {
        var userBlobName = $"users/{profileId}.png";
        var userBlob = _container.GetBlobClient(userBlobName);

        var blobExists = await userBlob.ExistsAsync(cancellationToken);
        var targetBlobName = blobExists.Value ? userBlobName : DefaultBlobName;

        return _container.GetBlobClient(targetBlobName);
    }

    private async Task<Uri> CreateReadSasUriAsync(
        BlobClient blobClient,
        CancellationToken cancellationToken)
    {
        return blobClient.CanGenerateSasUri
            ? CreateSharedKeyReadSasUri(blobClient)
            : await CreateUserDelegationReadSasUriAsync(blobClient, cancellationToken);
    }

    /// <summary>
    /// Creates a SAS signed with the Storage Account key.
    /// <para>
    /// <see cref="BlobClient.CanGenerateSasUri"/> is <c>true</c> when the client
    /// was created with Shared Key credentials, as in the local Azurite setup.
    /// </para>
    /// </summary>
    private Uri CreateSharedKeyReadSasUri(BlobClient blobClient)
    {
        var sasBuilder = CreateReadSasBuilder(blobClient.Name);

        return blobClient.GenerateSasUri(sasBuilder);
    }

    /// <summary>
    /// Creates a SAS signed with a user delegation key obtained through Microsoft Entra ID.
    /// <para>
    /// This path is used when <see cref="BlobClient.CanGenerateSasUri"/> is <c>false</c>,
    /// which is the case for a client authenticated with managed identity.
    /// </para>
    /// </summary>
    private async Task<Uri> CreateUserDelegationReadSasUriAsync(
        BlobClient blobClient,
        CancellationToken cancellationToken)
    {
        var sasBuilder = CreateReadSasBuilder(blobClient.Name);

        sasBuilder.Protocol = SasProtocol.Https;

        var userDelegationKey = await GetUserDelegationKeyAsync(cancellationToken);

        return blobClient.GenerateUserDelegationSasUri(sasBuilder, userDelegationKey);
    }

    private BlobSasBuilder CreateReadSasBuilder(string blobName)
    {
        var now = _clock.CurrentDate();

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _container.Name,
            BlobName = blobName,
            Resource = "b",
            StartsOn = now.AddMinutes(-5),
            ExpiresOn = now.AddMinutes(5)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        return sasBuilder;
    }

    private async Task<UserDelegationKey> GetUserDelegationKeyAsync(
        CancellationToken cancellationToken)
    {
        var now = _clock.CurrentDate();

        if (HasValidUserDelegationKey(now))
        {
            return _userDelegationKey!;
        }

        await _userDelegationKeyLock.WaitAsync(cancellationToken);

        try
        {
            now = _clock.CurrentDate();

            if (HasValidUserDelegationKey(now))
            {
                return _userDelegationKey!;
            }

            var startsOn = now.AddMinutes(-5);
            var expiresOn = now.Add(UserDelegationKeyLifetime);

            var response = await _serviceClient.GetUserDelegationKeyAsync(
                startsOn,
                expiresOn,
                cancellationToken);

            _userDelegationKey = response.Value;
            _userDelegationKeyExpiresOn = expiresOn;

            return _userDelegationKey;
        }
        finally
        {
            _userDelegationKeyLock.Release();
        }
    }

    private bool HasValidUserDelegationKey(DateTimeOffset now)
    {
        return _userDelegationKey is not null
            && _userDelegationKeyExpiresOn > now.Add(UserDelegationKeyRefreshWindow);
    }
}
