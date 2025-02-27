using Google.Cloud.Firestore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram2Todoist.Functions.Storage;
using Telegram2Todoist.Functions.Todoist;

namespace Telegram2Todoist.Functions;

public static class ContainerConfiguration
{
    public static IServiceProvider ConfigureServices()
    {
        var telegramAccessToken = GetConfigurationValue("TG_ACCESS_TOKEN");
        var todoistAuthClientId = GetConfigurationValue("TODOIST_AUTH_CLIENT_ID");
        var todoistAuthClientSecret = GetConfigurationValue("TODOIST_AUTH_CLIENT_SECRET");
        var googleCloudJsonCredentials =
            System.Text.Encoding.UTF8.GetString(
                Convert.FromBase64String(
                    GetConfigurationValue("GOOGLE_CLOUD_JSON_CREDENTIALS")));

        var services = new ServiceCollection();
        services
            .AddLogging(b => b.AddSimpleConsole(c => c.SingleLine = true));

        services
            .AddHttpClient(TodoistApiClient.HttpClientName);

        services
            .AddHttpClient(TodoistAuthClient.HttpClientName);

        services
            .AddSingleton(
                new FirestoreDbBuilder
                    {
                        ProjectId = "telegram2todoist",
                        DatabaseId = "telegram2todoist",
                        JsonCredentials = googleCloudJsonCredentials,
                    }
                    .Build())
            .AddSingleton<UsersStorage>()
            .AddSingleton<AuthStateStorage>()
            .AddSingleton<WebHookAsyncFunctionHandler>()
            .AddSingleton<OAuthAsyncFunctionHandler>()
            .AddSingleton(p => new TodoistAuthClient(
                p.GetRequiredKeyedService<IHttpClientFactory>(TodoistAuthClient.HttpClientName),
                todoistAuthClientId,
                todoistAuthClientSecret))
            .AddSingleton<TodoistApiClientFactory>()
            .AddSingleton<TodoistServiceFactory>()
            .AddSingleton<ITelegramBotClient>(new TelegramBotClient(telegramAccessToken));
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