namespace DuelApp.Modules.Questions.Application.Models;

public sealed record StoredImportFile(
    string BlobName,
    string ETag);