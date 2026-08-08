namespace DuelApp.Modules.Questions.Infrastructure.Configuration;

internal sealed class QuestionPublicationsServiceBusOptions
{
    public const string SectionName = "Azure:ServiceBus:QuestionPublications";
    
    public string? ConnectionString { get; init; }
    public string? FullyQualifiedNamespace { get; init; }
}
