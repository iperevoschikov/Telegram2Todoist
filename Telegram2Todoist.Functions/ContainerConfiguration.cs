using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram2Todoist.Functions.Todoist;

namespace Telegram2Todoist.Functions;

public static class ContainerConfiguration
{
    public static IServiceProvider ConfigureServices()
    {
        var todoistApiToken = GetConfigurationValue("TODOIST_API_TOKEN");
        var telegramAccessToken = GetConfigurationValue("TG_ACCESS_TOKEN");

        var services = new ServiceCollection();
        services
            .AddLogging(b => b.AddSimpleConsole(c => c.SingleLine = true))
            .AddHttpClient(TodoistApiClient.HttpClientName, client =>
            {
                client.BaseAddress = new Uri("https://api.todoist.com/rest/v2/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {todoistApiToken}");
            });
        services
            .AddSingleton<TodoistApiClient>()
            .AddSingleton(new TelegramBotClient(telegramAccessToken));
        return services.BuildServiceProvider();
    }

    private static string GetConfigurationValue(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);

        if (string.IsNullOrEmpty(value))
            throw new Exception($"{name} not found");

        return value;
    }
}