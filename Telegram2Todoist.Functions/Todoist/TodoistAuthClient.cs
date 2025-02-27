using System.Text.Json;
using System.Web;

namespace Telegram2Todoist.Functions.Todoist;

public class TodoistAuthClient(IHttpClientFactory httpClientFactory, string clientId, string clientSecret)
{
    public const string HttpClientName = "todoist_oauth";

    public string GetAuthUrl(string state)
    {
        var uriBuilder = new UriBuilder("https://todoist.com/oauth/authorize");
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["client_id"] = clientId;
        query["scope"] = "task:add";
        query["state"] = state;
        uriBuilder.Query = query.ToString();
        return uriBuilder.ToString();
    }

    public async Task<string> ObtainAccessToken(string code)
    {
        var client = httpClientFactory.CreateClient(HttpClientName);
        client.BaseAddress = new Uri("https://todoist.com/oauth/access_token");
        var response = await client.PostAsync("", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["code"] = code,
        }));
        response.EnsureSuccessStatusCode();
        var stringResponse = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TodoistAccessTokenResponse>(stringResponse);

        if (tokenResponse is null)
            throw new Exception($"Unable to deserialize response {stringResponse}");

        return tokenResponse.AccessToken;
    }
}