using System.Text;
using System.Text.Json;
using JetBrains.Annotations;

namespace Telegram2Todoist.Functions.Todoist;

[UsedImplicitly]
public class TodoistApiClient(IHttpClientFactory httpClientFactory, string apiToken)
{
    public const string HttpClientName = "todoist";

    private HttpClient CreateHttpClient()
    {
        var httpClient = httpClientFactory.CreateClient(HttpClientName);
        httpClient.BaseAddress = new Uri("https://api.todoist.com/rest/v2/");
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiToken}");
        return httpClient;
    }

    public async Task<TodoistProject[]> GetProjects()
    {
        var client = CreateHttpClient();
        var response = await client.GetAsync("projects");
        response.EnsureSuccessStatusCode();
        var text = await response.Content.ReadAsStringAsync();

        var projects = JsonSerializer.Deserialize<TodoistProject[]>(text);

        if (projects is null)
            throw new Exception($"Unable to deserialize response {text}");

        return projects;
    }

    public async Task CreateTaskAsync(string projectId, string title, string? description, DateOnly dueDate)
    {
        var httpClient = CreateHttpClient();
        var content = new StringContent(
            JsonSerializer.Serialize(
                new TodoistTask(
                    id: null,
                    content: title,
                    description,
                    projectId,
                    dueDate.ToString("yyyy-MM-dd"))),
            Encoding.UTF8,
            "application/json");

        content.Headers.Add("X-Request-Id", Guid.NewGuid().ToString());

        await httpClient.PostAsync("tasks", content);
    }
}