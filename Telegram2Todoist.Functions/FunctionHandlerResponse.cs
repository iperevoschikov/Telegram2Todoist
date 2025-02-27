using System.Text.Json.Serialization;

namespace Telegram2Todoist.Functions;

public class FunctionHandlerResponse(
    int statusCode,
    string? body,
    FunctionHandlerResponseHeaders headers,
    bool isBase64Encoded = false)
{
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; } = statusCode;

    [JsonPropertyName("body")]
    public string? Body { get; set; } = body;

    [JsonPropertyName("headers")]
    public FunctionHandlerResponseHeaders Headers { get; set; } = headers;

    [JsonPropertyName("isBase64Encoded")]
    public bool IsBase64Encoded { get; set; } = isBase64Encoded;

    public static FunctionHandlerResponse Ok(string? body = null)
    {
        return new FunctionHandlerResponse(200, body, new FunctionHandlerResponseHeaders());
    }
    
    public static FunctionHandlerResponse BadRequest()
    {
        return new FunctionHandlerResponse(400, null, new FunctionHandlerResponseHeaders());
    }
    
    public static FunctionHandlerResponse NotFound()
    {
        return new FunctionHandlerResponse(404, null, new FunctionHandlerResponseHeaders());
    }

    public static FunctionHandlerResponse Fail(string? error = null)
    {
        return new FunctionHandlerResponse(500, error, new FunctionHandlerResponseHeaders());
    }
}