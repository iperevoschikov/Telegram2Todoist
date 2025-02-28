using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Telegram2Todoist.Functions;

[PublicAPI]
public class FunctionHandlerResponseHeaders(string contentType = "application/json")
{
    [JsonPropertyName("Content-Type")]
    public string ContentType { get; set; } = contentType;

    [JsonPropertyName("Location")]
    public string? Location { get; set; }
}