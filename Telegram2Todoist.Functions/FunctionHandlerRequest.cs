using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Telegram2Todoist.Functions;

[PublicAPI]
public class FunctionHandlerRequest(
    string httpMethod,
    string body,
    Dictionary<string, string> queryStringParameters)
{
    [JsonPropertyName("httpMethod")]
    public string HttpMethod { get; init; } = httpMethod;

    [JsonPropertyName("body")]
    public string Body { get; init; } = body;

    [JsonPropertyName("queryStringParameters")]
    public Dictionary<string, string> Query { get; init; } = queryStringParameters;
}