using DuelApp.Modules.Questions.Domain.Questions.Enums;

namespace DuelApp.Modules.Questions.Domain.Questions.Entities;

public class QuestionImport
{
    public Guid Id { get; set; }   
    public string BlobName { get; set; }
    public string BlobETag { get; set; }
    public Guid RequestedBy  { get; set; }
    public ImportStatus Status { get; set; }
    public int TotalQuestionsCount { get; set; }
    public int ProcessedQuestionsCount { get; set; }
    public int RejectedQuestionsCount { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}
