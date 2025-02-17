namespace Telegram2Todoist.Functions.Todoist;

public class TodoistServiceFactory(TodoistApiClientFactory todoistApiClientFactory)
{
    public TodoistService Create(string apiToken) => new(todoistApiClientFactory, apiToken);
}