using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Telegram2Todoist.Functions;

[PublicAPI]
public class WebHookFunctionHandlerRequest(string httpMethod, string body)
{
    [JsonPropertyName("httpMethod")]
    public string HttpMethod { get; init; } = httpMethod;

    [JsonPropertyName("body")]
    public string Body { get; init; } = body;
}