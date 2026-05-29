using System;
using System.IO;
using System.Threading.Tasks;

namespace DuelApp.Modules.Users.Core.Services;

public interface IAvatarStorageService
{
    Task UploadAsync(string blobName, Stream content, string contentType);
    string GetBlobUrl(string blobName);
    string GetAvatarUrl(Guid userId);
}