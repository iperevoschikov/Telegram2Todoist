namespace Telegram2Todoist.Functions.Todoist;

public class TodoistApiClientFactory(IHttpClientFactory httpClientFactory)
{
    public TodoistApiClient Create(string apiToken)
    {
        return new TodoistApiClient(httpClientFactory, apiToken);
    }
}

