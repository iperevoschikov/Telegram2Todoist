using JetBrains.Annotations;

namespace Telegram2Todoist.Functions.Todoist;

[UsedImplicitly]
public class TodoistApiClientFactory(IHttpClientFactory httpClientFactory)
{
    public TodoistApiClient Create(string apiToken)
    {
        return new TodoistApiClient(httpClientFactory, apiToken);
    }
}