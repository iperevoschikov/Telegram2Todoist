using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram2Todoist.Functions.Storage;
using Telegram2Todoist.Functions.Todoist;

namespace Telegram2Todoist.Functions;

[UsedImplicitly]
public class OAuthAsyncFunctionHandler(
    TodoistAuthClient todoistAuth,
    AuthStateStorage authStateStorage,
    UsersStorage usersStorage,
    TelegramBotClient telegramClient,
    ILogger<OAuthAsyncFunctionHandler> logger
) : IAsyncFunctionHandler
{
    public async Task<FunctionHandlerResponse> HandleAsync(FunctionHandlerRequest request)
    {
        if (!request.QueryStringParameters.TryGetValue("code", out var code) ||
            !request.QueryStringParameters.TryGetValue("state", out var state))
            return FunctionHandlerResponse.BadRequest();

        var userId = await authStateStorage.GetUserIdForState(state);
        if (userId == null)
            return FunctionHandlerResponse.NotFound();

        var accessToken = await todoistAuth.ObtainAccessToken(code);
        await usersStorage.SetAccessTokenFor(userId.Value, accessToken);

        try
        {
            await telegramClient.SendTextMessageAsync(
                userId,
                "Бот успешно авторизован.\n" +
                "Просто отправляйте сюда сообщения, а я буду превращать их в задачи.");
        }
        catch
        {
            logger.LogWarning("Unable to send telegram message for user {UserId}", userId);
        }

        return FunctionHandlerResponse.Redirect("https://t.me/todoist_forward_bot");
    }
}