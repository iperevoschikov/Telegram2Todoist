using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Telegram2Todoist.Functions;

[PublicAPI]
public class WebHookFunctionHandlerResponseHeaders(string contentType = "application/json")
{
    [JsonPropertyName("Content-Type")]
    public string ContentType { get; set; } = contentType;
}