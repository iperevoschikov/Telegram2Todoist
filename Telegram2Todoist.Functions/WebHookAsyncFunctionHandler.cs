using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram2Todoist.Functions.Storage;
using Telegram2Todoist.Functions.Todoist;

namespace Telegram2Todoist.Functions;

[UsedImplicitly]
public class WebHookAsyncFunctionHandler(
    ITelegramBotClient telegramClient,
    UsersStorage usersStorage,
    ILogger<WebHookFunctionHandler> logger,
    TodoistApiClientFactory todoistApiClientFactory,
    TodoistAuthClient todoistAuthClient,
    AuthStateStorage authStateStorage
) : IAsyncFunctionHandler
{
    public async Task<FunctionHandlerResponse> HandleAsync(FunctionHandlerRequest request)
    {
        var update = JsonConvert.DeserializeObject<Update>(request.Body)!;
        logger.LogInformation("Received webhook update: {Update},", update.Id);

        if (update.Message?.Text?.StartsWith("DEBUG") ?? false)
        {
            logger.LogInformation("Request body {Body}", request.Body);
        }

        var message = update.Message;

        try
        {
            if (message == null)
                throw new NullReferenceException("Message is null");
            if (message.From?.Id == null)
                throw new NullReferenceException("From is null");

            var userId = message.From.Id;
            var authToken = await usersStorage.GetAccessTokenFor(userId);

            if (string.IsNullOrEmpty(authToken))
            {
                var state = await authStateStorage.CreateAuthState(userId);

                await telegramClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Для работы необходимо дать боту разрешение создавать задачи в вашем Todoist",
                    replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton
                        .WithUrl(
                            "Авторизовать бота",
                            todoistAuthClient.GetAuthUrl(state))));

                return FunctionHandlerResponse.Ok();
            }

            var title = BuildContactName(update.Message?.ForwardFrom)
                        ?? (update.Message?.ForwardSenderName == null
                            ? null
                            : Regex.Unescape(update.Message?.ForwardSenderName!))
                        ?? BuildContactName(update.Message?.From)
                        ?? "Задача из Telegram";
            var description = TelegramMessageEntitiesFormatter.ToMarkdown(update.Message);

            await todoistApiClientFactory
                .Create(authToken)
                .CreateTaskAsync(
                    title,
                    description,
                    DateOnly.FromDateTime(DateTime.UtcNow.AddHours(5)));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception occurred {Message}", e.Message);

            await telegramClient.SendTextMessageAsync(
                message!.Chat.Id,
                "Не смог создать задачу",
                replyToMessageId: message.MessageId);
        }

        return FunctionHandlerResponse.Ok();
    }

    [ContractAnnotation("null => null;notnull=>notnull")]
    private static string? BuildContactName(User? user)
    {
        return user == null
            ? null
            : $"{user.FirstName} {user.LastName}";
    }
}