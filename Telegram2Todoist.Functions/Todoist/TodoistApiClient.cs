using System.Text;
using System.Text.Json;
using JetBrains.Annotations;

namespace Telegram2Todoist.Functions.Todoist;

[UsedImplicitly]
public class TodoistApiClient(IHttpClientFactory httpClientFactory)
{
    public const string HttpClientName = "todoist";

    public async Task<TodoistProject[]> GetProjects()
    {
        var client = httpClientFactory.CreateClient(HttpClientName);
        var response = await client.GetAsync("projects");
        response.EnsureSuccessStatusCode();
        var text = await response.Content.ReadAsStringAsync();

        var projects = JsonSerializer.Deserialize<TodoistProject[]>(text);

        if (projects is null)
            throw new Exception($"Unable to deserialize response {text}");

        return projects;
    }

    public async Task CreateTaskAsync(string projectId, string title, string? description)
    {
        var httpClient = httpClientFactory.CreateClient(HttpClientName);
        var content = new StringContent(
            JsonSerializer.Serialize(
                new TodoistTask(
                    id: null,
                    content: title,
                    description,
                    projectId)),
            Encoding.UTF8,
            "application/json");

        content.Headers.Add("X-Request-Id", Guid.NewGuid().ToString());

        await httpClient.PostAsync("tasks", content);
    }
}