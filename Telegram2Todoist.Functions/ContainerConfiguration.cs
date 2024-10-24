using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram2Todoist.Functions.Todoist;

namespace Telegram2Todoist.Functions;

public static class ContainerConfiguration
{
    public static IServiceProvider ConfigureServices()
    {
        var todoistApiToken = Environment.GetEnvironmentVariable("TODOIST_API_TOKEN");

        if (string.IsNullOrEmpty(todoistApiToken))
            throw new Exception("Todoist api token not found");

        var services = new ServiceCollection();
        services
            .AddLogging(b => b.AddSimpleConsole(c => c.SingleLine = true))
            .AddHttpClient(TodoistApiClient.HttpClientName, client =>
            {
                client.BaseAddress = new Uri("https://api.todoist.com/rest/v2/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {todoistApiToken}");
            })
            ;
        return services.BuildServiceProvider();
    }
}