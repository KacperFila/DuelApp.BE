using Azure;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

var options = InspectorOptions.Parse(args);

if (options.ShowHelp)
{
    InspectorOptions.PrintHelp();
    return 0;
}

try
{
    await PrintQueueStatisticsAsync(options);

    await using var client = new ServiceBusClient(options.ConnectionString);
    await using var receiver = client.CreateReceiver(
        options.QueueName,
        new ServiceBusReceiverOptions
        {
            SubQueue = options.ReadDeadLetterQueue ? SubQueue.DeadLetter : SubQueue.None
        });

    var messages = await receiver.PeekMessagesAsync(options.MessageCount);

    if (messages.Count == 0)
    {
        Console.WriteLine(options.ReadDeadLetterQueue
            ? "The dead-letter queue is empty."
            : "The queue is empty.");
        return 0;
    }

    Console.WriteLine(options.ReadDeadLetterQueue
        ? $"Peeked messages from the dead-letter queue: {messages.Count}"
        : $"Peeked messages: {messages.Count}");

    foreach (var message in messages)
    {
        Console.WriteLine();
        Console.WriteLine($"Sequence number: {message.SequenceNumber}");
        Console.WriteLine($"Message ID: {message.MessageId}");
        Console.WriteLine($"Enqueued: {message.EnqueuedTime:O}");
        Console.WriteLine($"Delivery count: {message.DeliveryCount}");
        Console.WriteLine($"Subject: {message.Subject ?? "<none>"}");
        Console.WriteLine($"Body: {message.Body}");

        if (message.ApplicationProperties.Count > 0)
        {
            Console.WriteLine("Application properties:");

            foreach (var (key, value) in message.ApplicationProperties)
            {
                Console.WriteLine($"  {key}: {value ?? "<null>"}");
            }
        }
    }

    Console.WriteLine();
    Console.WriteLine("Messages were peeked only; none were received or completed.");
    return 0;
}
catch (ServiceBusException exception)
{
    Console.Error.WriteLine($"Service Bus error: {exception.Message}");
    return 1;
}
catch (RequestFailedException exception)
{
    Console.Error.WriteLine($"Service Bus administration error: {exception.Message}");
    return 1;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Unexpected error: {exception.Message}");
    return 1;
}

static async Task PrintQueueStatisticsAsync(InspectorOptions options)
{
    try
    {
        var administrationClient = new ServiceBusAdministrationClient(options.AdministrationConnectionString);
        var runtimeProperties = await administrationClient.GetQueueRuntimePropertiesAsync(options.QueueName);

        Console.WriteLine($"Queue: {options.QueueName}");
        Console.WriteLine($"Active messages: {runtimeProperties.Value.ActiveMessageCount}");
        Console.WriteLine($"Dead-letter messages: {runtimeProperties.Value.DeadLetterMessageCount}");
        Console.WriteLine($"Scheduled messages: {runtimeProperties.Value.ScheduledMessageCount}");
        Console.WriteLine();
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"Queue statistics are unavailable: {exception.Message}");
        Console.Error.WriteLine("Continuing with message peeking.");
        Console.Error.WriteLine();
    }
}

internal sealed class InspectorOptions
{
    private const string LocalConnectionString =
        "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";

    private const string LocalAdministrationConnectionString =
        "Endpoint=sb://localhost:5300;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";

    public string QueueName { get; private set; } = "question-imports";

    public int MessageCount { get; private set; } = 10;

    public bool ReadDeadLetterQueue { get; private set; }

    public bool ShowHelp { get; private set; }

    public string ConnectionString { get; private set; } = LocalConnectionString;

    public string AdministrationConnectionString { get; private set; } = LocalAdministrationConnectionString;

    public static InspectorOptions Parse(string[] args)
    {
        var options = new InspectorOptions();

        for (var index = 0; index < args.Length; index++)
        {
            switch (args[index])
            {
                case "--queue":
                    options.QueueName = GetArgumentValue(args, ref index, "--queue");
                    break;
                case "--count":
                    var count = GetArgumentValue(args, ref index, "--count");

                    if (!int.TryParse(count, out var parsedCount) || parsedCount < 1)
                    {
                        throw new ArgumentException("--count must be a positive integer.");
                    }

                    options.MessageCount = parsedCount;
                    break;
                case "--dead-letter":
                    options.ReadDeadLetterQueue = true;
                    break;
                case "--connection":
                    options.ConnectionString = GetArgumentValue(args, ref index, "--connection");
                    break;
                case "--admin-connection":
                    options.AdministrationConnectionString = GetArgumentValue(args, ref index, "--admin-connection");
                    break;
                case "--help" or "-h":
                    options.ShowHelp = true;
                    break;
                default:
                    throw new ArgumentException($"Unknown argument: {args[index]}");
            }
        }

        return options;
    }

    public static void PrintHelp()
    {
        Console.WriteLine("Usage: dotnet run --project tools/DuelApp.ServiceBusInspector -- [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --queue <name>                 Queue to inspect (default: question-imports)");
        Console.WriteLine("  --count <number>               Maximum messages to peek (default: 10)");
        Console.WriteLine("  --dead-letter                  Inspect the dead-letter queue");
        Console.WriteLine("  --connection <connection>      AMQP connection string for peeking messages");
        Console.WriteLine("  --admin-connection <connection> Connection string for queue statistics");
        Console.WriteLine("  --help, -h                     Show this help");
    }

    private static string GetArgumentValue(string[] args, ref int index, string argumentName)
    {
        if (++index >= args.Length || string.IsNullOrWhiteSpace(args[index]))
        {
            throw new ArgumentException($"{argumentName} requires a value.");
        }

        return args[index];
    }
}
