using Google.Cloud.Firestore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram2Todoist.Functions.Storage;
using Telegram2Todoist.Functions.Todoist;

namespace Telegram2Todoist.Functions;

public static class ContainerConfiguration
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        var telegramAccessToken = GetConfigurationValue("TG_ACCESS_TOKEN");
        var todoistAuthClientId = GetConfigurationValue("TODOIST_AUTH_CLIENT_ID");
        var todoistAuthClientSecret = GetConfigurationValue("TODOIST_AUTH_CLIENT_SECRET");
        var googleCloudJsonCredentials = System.Text.Encoding.UTF8.GetString(
            Convert.FromBase64String(GetConfigurationValue("GOOGLE_CLOUD_JSON_CREDENTIALS"))
        );

        services.AddLogging(b =>
        {
            b.AddSimpleConsole(c => c.SingleLine = true);
            b.SetMinimumLevel(LogLevel.Information);
        });

        services.AddHttpClient(TodoistApiClient.HttpClientName);

        services.AddHttpClient(TodoistAuthClient.HttpClientName);

        services
            .AddSingleton(
                new FirestoreDbBuilder
                {
                    ProjectId = "telegram2todoist",
                    DatabaseId = "telegram2todoist",
                    JsonCredentials = googleCloudJsonCredentials,
                }.Build()
            )
            .AddSingleton<UsersStorage>()
            .AddSingleton<AuthStateStorage>()
            .AddSingleton<WebHookAsyncFunctionHandler>()
            .AddSingleton<OAuthAsyncFunctionHandler>()
            .AddSingleton(
                new TodoistAuthClientSettings(todoistAuthClientId, todoistAuthClientSecret)
            )
            .AddSingleton<TodoistAuthClient>()
            .AddSingleton<TodoistApiClientFactory>()
            .AddSingleton<ITelegramBotClient>(new TelegramBotClient(telegramAccessToken));
        return services;
    }

    private static string GetConfigurationValue(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);

        if (string.IsNullOrEmpty(value))
            throw new Exception($"{name} not found");

        return value;
    }
}
