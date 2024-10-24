using System.Text.Json.Serialization;

namespace Telegram2Todoist.Functions;

public class WebHookFunctionHandlerResponse(
    int statusCode,
    string? body,
    WebHookFunctionHandlerResponseHeaders headers,
    bool isBase64Encoded = false)
{
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; } = statusCode;

    [JsonPropertyName("body")]
    public string? Body { get; set; } = body;

    [JsonPropertyName("headers")]
    public WebHookFunctionHandlerResponseHeaders Headers { get; set; } = headers;

    [JsonPropertyName("isBase64Encoded")]
    public bool IsBase64Encoded { get; set; } = isBase64Encoded;

    public static WebHookFunctionHandlerResponse Ok(string? body)
    {
        return new WebHookFunctionHandlerResponse(200, body, new WebHookFunctionHandlerResponseHeaders());
    }
}