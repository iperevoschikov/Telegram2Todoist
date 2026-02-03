using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram2Todoist.Functions.Storage;
using Telegram2Todoist.Functions.Todoist;
using YandexCloudFunctions.Net.Sdk;
using YandexCloudFunctions.Net.Sdk.Webhook;

namespace Telegram2Todoist.Functions;

public class OAuthAsyncFunctionHandler() : WebhookFunctionHandler(HandleAsync)
{
    public static async Task<WebhookHandlerResponse> HandleAsync(
        WebhookHandlerRequest request,
        TodoistAuthClient todoistAuth,
        AuthStateStorage authStateStorage,
        UsersStorage usersStorage,
        ITelegramBotClient telegramClient,
        ILogger<OAuthAsyncFunctionHandler> logger
    )
    {
        if (
            !request.queryStringParameters.TryGetValue("code", out var code)
            || !request.queryStringParameters.TryGetValue("state", out var state)
        )
            return WebhookHandlerResponses.BadRequest();

        var userId = await authStateStorage.GetUserIdForState(state);
        if (userId == null)
            return WebhookHandlerResponses.NotFound();

        var accessToken = await todoistAuth.ObtainAccessToken(code);
        await usersStorage.SetAccessTokenFor(userId.Value, accessToken);

        try
        {
            await telegramClient.SendMessage(
                userId,
                "Бот успешно авторизован.\n"
                    + "Просто отправляйте сюда сообщения, а я буду превращать их в задачи."
            );
        }
        catch
        {
            logger.LogWarning("Unable to send telegram message for user {UserId}", userId);
        }

        return WebhookHandlerResponses.Redirect("https://t.me/todoist_forward_bot");
    }

    protected override void ConfigureServices(IServiceCollection serviceCollection) =>
        serviceCollection.ConfigureServices();
}
