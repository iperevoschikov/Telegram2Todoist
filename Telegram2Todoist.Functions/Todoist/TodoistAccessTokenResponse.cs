using System.Text.Json.Serialization;

namespace Telegram2Todoist.Functions.Todoist;

public class TodoistAccessTokenResponse(string accessToken, string tokenType)
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = accessToken;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = tokenType;
}

