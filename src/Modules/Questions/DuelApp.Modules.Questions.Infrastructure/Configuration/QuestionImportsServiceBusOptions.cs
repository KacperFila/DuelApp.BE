namespace DuelApp.Modules.Questions.Infrastructure.Configuration;

internal sealed class QuestionImportsServiceBusOptions
{
    public const string SectionName = "Azure:ServiceBus:QuestionImports";

    public string? ConnectionString { get; init; }
    public string? FullyQualifiedNamespace { get; init; }
    public string QueueName { get; init; } = "question-imports";
}
