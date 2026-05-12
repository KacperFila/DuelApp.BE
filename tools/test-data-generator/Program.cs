using System.Text.Encodings.Web;
using System.Text.Json;

var questions = QuestionGenerator.Services.QuestionGenerator.GenerateMathQuestions(20);

var json = JsonSerializer.Serialize(questions, new JsonSerializerOptions
{
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    WriteIndented = true
});

var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
var path = Path.Combine(Directory.GetCurrentDirectory(), $"questions-{timestamp}.json");
File.WriteAllText(path, json);

Console.WriteLine($"Generated: {path}");