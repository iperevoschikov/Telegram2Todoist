namespace Telegram2Todoist.Functions.Todoist;

public class TodoistService(TodoistApiClientFactory todoistApiClientFactory, string apiToken)
{
    private readonly TodoistApiClient todoistApiClient = todoistApiClientFactory.Create(apiToken);

    public async Task CreateTask(string title, string description)
    {
        var inbox = (await todoistApiClient.GetProjects())
            .FirstOrDefault(p => p.IsInboxProject);

        if (inbox == null)
            throw new Exception("Inbox project not found");


        await todoistApiClient.CreateTaskAsync(
            inbox.Id,
            title,
            description,
            DateOnly.FromDateTime(DateTime.UtcNow.AddHours(5)));
    }
}