using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Telegram2Todoist.Functions.Todoist;

[PublicAPI]
public class TodoistAccessTokenResponse(string accessToken, string tokenType)
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = accessToken;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = tokenType;
}